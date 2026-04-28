using Torneio.Application.Common;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.DTOs.RegistroPublicoMembro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class RegistroPublicoMembroServico : IRegistroPublicoMembroServico
{
    private const int IntervaloMinimoReenvioSegundos = 60;
    private const int ExpiracaoCodigoMinutos = 10;
    private const int MaxTentativasValidacao = 5;

    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IRegistroPublicoMembroRepositorio _registroRepositorio;
    private readonly IFinanceiroTorneioServico _financeiroServico;
    private readonly ISmsVerificacaoServico _smsVerificacaoServico;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly IPasswordHasher _passwordHasher;

    public RegistroPublicoMembroServico(
        ITorneioRepositorio torneioRepositorio,
        IMembroRepositorio membroRepositorio,
        IRegistroPublicoMembroRepositorio registroRepositorio,
        IFinanceiroTorneioServico financeiroServico,
        ISmsVerificacaoServico smsVerificacaoServico,
        ILogAuditoriaServico logAuditoriaServico,
        IPasswordHasher passwordHasher)
    {
        _torneioRepositorio = torneioRepositorio;
        _membroRepositorio = membroRepositorio;
        _registroRepositorio = registroRepositorio;
        _financeiroServico = financeiroServico;
        _smsVerificacaoServico = smsVerificacaoServico;
        _logAuditoriaServico = logAuditoriaServico;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegistroPublicoMembroSolicitadoDto> SolicitarCodigo(
        Guid torneioId,
        string nomeTorneio,
        SolicitarRegistroPublicoMembroDto dto,
        string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new InvalidOperationException("Informe o nome do pescador.");
        if (string.IsNullOrWhiteSpace(dto.Celular))
            throw new InvalidOperationException("Informe o celular.");

        var torneio = await _torneioRepositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException("Torneio nao encontrado.");
        ValidarDisponibilidade(torneio);

        var celularNormalizado = NormalizarCelular(dto.Celular);
        var membroExistente = await _membroRepositorio.ObterPorCelularNormalizado(torneioId, celularNormalizado);
        if (membroExistente is not null)
            throw new InvalidOperationException("Ja existe um pescador cadastrado com este celular neste torneio.");

        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(ExpiracaoCodigoMinutos);
        var registro = await _registroRepositorio.ObterUltimoPorCelular(torneioId, celularNormalizado);

        if (registro is not null)
        {
            if (registro.Status == StatusRegistroPublicoMembro.Pendente &&
                registro.UltimoEnvioEm.AddSeconds(IntervaloMinimoReenvioSegundos) > agora)
            {
                throw new InvalidOperationException("Aguarde alguns segundos antes de solicitar um novo codigo.");
            }

            registro.AtualizarDados(dto.Nome, dto.Celular, celularNormalizado, dto.TamanhoCamisa);
            registro.RegistrarReenvio(agora, expiraEm);
            await _registroRepositorio.Atualizar(registro);
        }
        else
        {
            registro = RegistroPublicoMembro.Criar(
                torneioId,
                dto.Nome,
                dto.Celular,
                celularNormalizado,
                dto.TamanhoCamisa,
                agora,
                expiraEm);
            await _registroRepositorio.Adicionar(registro);
        }

        await _smsVerificacaoServico.EnviarCodigo(celularNormalizado);

        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = nomeTorneio,
            Categoria = CategoriaLog.Membros,
            Acao = "SolicitarRegistroPublicoMembro",
            Descricao = $"Codigo de validacao enviado para cadastro publico de membro | Nome: {dto.Nome.Trim()} | Celular: {MascararCelular(dto.Celular)}",
            UsuarioNome = dto.Nome.Trim(),
            UsuarioPerfil = "Publico",
            IpAddress = ipAddress,
        });

        return new RegistroPublicoMembroSolicitadoDto
        {
            RegistroId = registro.Id,
            CelularMascarado = MascararCelular(dto.Celular),
            ExpiraEm = registro.ExpiraEm,
            Mensagem = "Codigo enviado por SMS.",
        };
    }

    public async Task<MembroDto> ConfirmarCodigo(
        Guid torneioId,
        string nomeTorneio,
        ConfirmarRegistroPublicoMembroDto dto,
        string? ipAddress)
    {
        var torneio = await _torneioRepositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException("Torneio nao encontrado.");
        ValidarDisponibilidade(torneio);

        var registro = await _registroRepositorio.ObterPorId(dto.RegistroId)
            ?? throw new KeyNotFoundException("Solicitacao de cadastro nao encontrada.");

        if (registro.TorneioId != torneioId)
            throw new KeyNotFoundException("Solicitacao de cadastro nao encontrada.");

        if (registro.Status == StatusRegistroPublicoMembro.Confirmado && registro.MembroId.HasValue)
        {
            var membroConfirmado = await _membroRepositorio.ObterPorId(registro.MembroId.Value);
            if (membroConfirmado is not null)
                return ParaDto(membroConfirmado);
        }

        if (registro.ExpiraEm < DateTime.UtcNow)
        {
            registro.Expirar();
            await _registroRepositorio.Atualizar(registro);
            throw new InvalidOperationException("O codigo expirou. Solicite um novo envio.");
        }

        if (registro.TentativasValidacao >= MaxTentativasValidacao)
            throw new InvalidOperationException("Quantidade maxima de tentativas atingida. Solicite um novo codigo.");

        registro.RegistrarTentativaValidacao();
        await _registroRepositorio.Atualizar(registro);

        var aprovado = await _smsVerificacaoServico.ValidarCodigo(registro.CelularNormalizado, dto.Codigo.Trim());
        if (!aprovado)
            throw new InvalidOperationException("Codigo invalido.");

        ValidarCredenciais(dto.Usuario, dto.Senha);
        var membroExistente = await _membroRepositorio.ObterPorCelularNormalizado(torneioId, registro.CelularNormalizado);
        await ValidarUsuarioDisponivel(torneioId, dto.Usuario, membroExistente?.Id);

        if (membroExistente is not null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Usuario) && !string.IsNullOrWhiteSpace(dto.Senha))
            {
                membroExistente.AtualizarCredenciais(dto.Usuario, _passwordHasher.Hash(dto.Senha));
                await _membroRepositorio.Atualizar(membroExistente);
            }

            registro.Confirmar(membroExistente.Id, DateTime.UtcNow);
            await _registroRepositorio.Atualizar(registro);
            return ParaDto(membroExistente);
        }

        var senhaHash = string.IsNullOrWhiteSpace(dto.Senha)
            ? null
            : _passwordHasher.Hash(dto.Senha);

        var membro = Membro.Criar(torneioId, registro.Nome, null, registro.Celular, registro.TamanhoCamisa, dto.Usuario, senhaHash);
        await _membroRepositorio.Adicionar(membro);
        await _financeiroServico.SincronizarParcelas(torneioId, [membro.Id], true);

        registro.Confirmar(membro.Id, DateTime.UtcNow);
        await _registroRepositorio.Atualizar(registro);

        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = nomeTorneio,
            Categoria = CategoriaLog.Membros,
            Acao = "ConfirmarRegistroPublicoMembro",
            Descricao = $"Membro criado via cadastro publico com validacao por SMS | Nome: {membro.Nome} | Celular: {MascararCelular(membro.Celular)} | MembroId: {membro.Id}",
            UsuarioNome = membro.Nome,
            UsuarioPerfil = "Publico",
            IpAddress = ipAddress,
        });

        return ParaDto(membro);
    }

    private static void ValidarDisponibilidade(TorneioEntity torneio)
    {
        if (!torneio.Ativo)
            throw new InvalidOperationException("O torneio nao esta ativo.");

        if (!torneio.PermitirRegistroPublicoMembro)
            throw new InvalidOperationException("O cadastro publico de pescadores nao esta habilitado para este torneio.");
    }

    private static void ValidarCredenciais(string? usuario, string? senha)
    {
        if (string.IsNullOrWhiteSpace(usuario) && string.IsNullOrWhiteSpace(senha))
            return;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            throw new InvalidOperationException("Informe usuario e senha juntos para habilitar o acesso do pescador.");
        if (senha.Trim().Length < 6)
            throw new InvalidOperationException("A senha deve ter pelo menos 6 caracteres.");
    }

    private async Task ValidarUsuarioDisponivel(Guid torneioId, string? usuario, Guid? membroIdAtual)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            return;

        var existente = await _membroRepositorio.ObterPorUsuario(torneioId, usuario);
        if (existente is not null && existente.Id != membroIdAtual)
            throw new InvalidOperationException("Ja existe um pescador com este usuario neste torneio.");
    }

    private static string NormalizarCelular(string valor)
    {
        var digitos = new string(valor.Where(char.IsDigit).ToArray());
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

    private static MembroDto ParaDto(Membro membro) => new()
    {
        Id = membro.Id,
        TorneioId = membro.TorneioId,
        Nome = membro.Nome,
        FotoUrl = membro.FotoUrl,
        Celular = membro.Celular,
        TamanhoCamisa = membro.TamanhoCamisa,
        Usuario = membro.Usuario,
        PossuiSenha = !string.IsNullOrWhiteSpace(membro.SenhaHash),
    };
}
