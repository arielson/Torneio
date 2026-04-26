using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class MembroServico : IMembroServico
{
    private readonly IMembroRepositorio _repositorio;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<CriarMembroDto> _validador;
    private readonly IFinanceiroTorneioServico _financeiroServico;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISmsVerificacaoServico _smsVerificacaoServico;
    private readonly ILogAuditoriaServico _logAuditoriaServico;

    public MembroServico(
        IMembroRepositorio repositorio,
        ITenantContext tenantContext,
        IValidator<CriarMembroDto> validador,
        IFinanceiroTorneioServico financeiroServico,
        IPasswordHasher passwordHasher,
        ISmsVerificacaoServico smsVerificacaoServico,
        ILogAuditoriaServico logAuditoriaServico)
    {
        _repositorio = repositorio;
        _tenantContext = tenantContext;
        _validador = validador;
        _financeiroServico = financeiroServico;
        _passwordHasher = passwordHasher;
        _smsVerificacaoServico = smsVerificacaoServico;
        _logAuditoriaServico = logAuditoriaServico;
    }

    public async Task<MembroDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        if (entidade is null || entidade.TorneioId != _tenantContext.TorneioId)
            return null;

        return ParaDto(entidade);
    }

    public async Task<IEnumerable<MembroDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarPorTorneio(_tenantContext.TorneioId);
        return lista.OrderBy(m => m.Nome, StringComparer.CurrentCultureIgnoreCase).Select(ParaDto);
    }

    public async Task<MembroDto> Criar(CriarMembroDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        await ValidarUsuarioDisponivel(dto.TorneioId, dto.Usuario);

        var senhaHash = string.IsNullOrWhiteSpace(dto.Senha)
            ? null
            : _passwordHasher.Hash(dto.Senha);

        var entidade = Membro.Criar(dto.TorneioId, dto.Nome, dto.FotoUrl, dto.Celular, dto.TamanhoCamisa, dto.Usuario, senhaHash,
            deveAlterarSenha: senhaHash != null);
        await _repositorio.Adicionar(entidade);
        await _financeiroServico.SincronizarParcelas(dto.TorneioId);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarMembroDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Membro '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Membro '{id}' nao encontrado.");

        entidade.AtualizarNome(dto.Nome);
        if (dto.FotoUrl is not null)
            entidade.AtualizarFoto(dto.FotoUrl);
        entidade.AtualizarCelular(dto.Celular);
        entidade.AtualizarTamanhoCamisa(dto.TamanhoCamisa);
        await ValidarUsuarioDisponivel(entidade.TorneioId, dto.Usuario, id);
        var senhaHash = string.IsNullOrWhiteSpace(dto.Senha)
            ? null
            : _passwordHasher.Hash(dto.Senha);
        entidade.AtualizarCredenciais(dto.Usuario, senhaHash);

        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Membro '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Membro '{id}' nao encontrado.");

        await _financeiroServico.ValidarRemocaoMembro(id);
        await _repositorio.Remover(entidade.Id);
        await _financeiroServico.SincronizarParcelas(entidade.TorneioId);
    }

    public async Task<(int total, List<string> fotosParaRemover)> RemoverTodos() =>
        await _repositorio.RemoverTodos(_tenantContext.TorneioId);

    public async Task<RecuperacaoSenhaMembroSolicitadaDto> SolicitarRecuperacaoSenha(
        Guid torneioId,
        string nomeTorneio,
        SolicitarRecuperacaoSenhaMembroDto dto,
        string? ipAddress)
    {
        var membro = await _repositorio.ObterPorUsuario(torneioId, dto.Usuario)
            ?? throw new InvalidOperationException("Usuario ou celular invalido.");

        if (NormalizarCelular(membro.Celular) != NormalizarCelular(dto.Celular))
            throw new InvalidOperationException("Usuario ou celular invalido.");

        await _smsVerificacaoServico.EnviarCodigo(NormalizarCelular(dto.Celular));

        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = nomeTorneio,
            Categoria = CategoriaLog.Usuarios,
            Acao = "SolicitarRecuperacaoSenhaMembro",
            Descricao = $"Codigo de recuperacao de senha enviado para pescador | MembroId: {membro.Id} | Usuario: {membro.Usuario} | Celular: {MascararCelular(membro.Celular)}",
            UsuarioNome = membro.Nome,
            UsuarioPerfil = "Publico",
            IpAddress = ipAddress
        });

        return new RecuperacaoSenhaMembroSolicitadaDto
        {
            CelularMascarado = MascararCelular(membro.Celular),
            Mensagem = "Codigo enviado por SMS."
        };
    }

    public async Task RedefinirSenha(
        Guid torneioId,
        string nomeTorneio,
        ConfirmarRecuperacaoSenhaMembroDto dto,
        string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(dto.NovaSenha))
            throw new InvalidOperationException("Informe a nova senha.");

        var membro = await _repositorio.ObterPorUsuario(torneioId, dto.Usuario)
            ?? throw new InvalidOperationException("Usuario ou celular invalido.");

        var celularNormalizado = NormalizarCelular(dto.Celular);
        if (NormalizarCelular(membro.Celular) != celularNormalizado)
            throw new InvalidOperationException("Usuario ou celular invalido.");

        var aprovado = await _smsVerificacaoServico.ValidarCodigo(celularNormalizado, dto.Codigo.Trim());
        if (!aprovado)
            throw new InvalidOperationException("Codigo invalido.");

        membro.AtualizarCredenciais(membro.Usuario, _passwordHasher.Hash(dto.NovaSenha));
        await _repositorio.Atualizar(membro);

        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = nomeTorneio,
            Categoria = CategoriaLog.Usuarios,
            Acao = "RedefinirSenhaMembro",
            Descricao = $"Senha do pescador redefinida via recuperacao publica | MembroId: {membro.Id} | Usuario: {membro.Usuario}",
            UsuarioNome = membro.Nome,
            UsuarioPerfil = "Publico",
            IpAddress = ipAddress
        });
    }

    private static MembroDto ParaDto(Membro e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Celular = e.Celular,
        TamanhoCamisa = e.TamanhoCamisa,
        Usuario = e.Usuario,
        PossuiSenha = !string.IsNullOrWhiteSpace(e.SenhaHash)
    };

    private async Task ValidarUsuarioDisponivel(Guid torneioId, string? usuario, Guid? membroIdAtual = null)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            return;

        var existente = await _repositorio.ObterPorUsuario(torneioId, usuario);
        if (existente is not null && existente.Id != membroIdAtual)
            throw new InvalidOperationException("Ja existe um pescador com este usuario neste torneio.");
    }

    private static string NormalizarCelular(string? valor)
    {
        var digitos = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digitos))
            throw new InvalidOperationException("Informe um celular valido.");

        if (digitos.Length == 10 || digitos.Length == 11)
            return $"+55{digitos}";

        if (digitos.StartsWith("55") && (digitos.Length == 12 || digitos.Length == 13))
            return $"+{digitos}";

        if (digitos.Length < 10 || digitos.Length > 15)
            throw new InvalidOperationException("Informe um celular valido.");

        return $"+{digitos}";
    }

    private static string MascararCelular(string? celular)
    {
        var digitos = new string((celular ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length <= 4)
            return celular ?? string.Empty;

        var prefixo = digitos.Length > 2 ? digitos[..2] : digitos;
        var sufixo = digitos.Length > 2 ? digitos[^2..] : digitos;
        return $"({prefixo}) *****-{sufixo}";
    }
}
