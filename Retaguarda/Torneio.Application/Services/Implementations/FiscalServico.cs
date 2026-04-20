using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class FiscalServico : IFiscalServico
{
    private readonly IFiscalRepositorio _repositorio;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CriarFiscalDto> _validador;

    public FiscalServico(
        IFiscalRepositorio repositorio,
        ITenantContext tenantContext,
        IPasswordHasher passwordHasher,
        IValidator<CriarFiscalDto> validador)
    {
        _repositorio = repositorio;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
        _validador = validador;
    }

    public async Task<FiscalDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        if (entidade is null || entidade.TorneioId != _tenantContext.TorneioId)
            return null;

        return ParaDto(entidade);
    }

    public async Task<IEnumerable<FiscalDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarPorTorneio(_tenantContext.TorneioId);
        return lista.Select(ParaDto);
    }

    public async Task<FiscalDto> Criar(CriarFiscalDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorUsuario(dto.Usuario, dto.TorneioId);
        if (existente is not null)
            throw new InvalidOperationException($"Usuario '{dto.Usuario}' ja existe neste torneio.");

        var entidade = Fiscal.Criar(
            dto.TorneioId,
            dto.Nome, dto.Usuario, _passwordHasher.Hash(dto.Senha), dto.FotoUrl);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarFiscalDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Fiscal '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Fiscal '{id}' nao encontrado.");

        if (!string.Equals(entidade.Usuario, dto.Usuario, StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _repositorio.ObterPorUsuario(dto.Usuario, entidade.TorneioId);
            if (existente is not null && existente.Id != id)
                throw new InvalidOperationException($"Usuario '{dto.Usuario}' ja existe neste torneio.");
        }

        entidade.AtualizarNome(dto.Nome);
        entidade.AtualizarUsuario(dto.Usuario);
        if (dto.FotoUrl is not null)
            entidade.AtualizarFoto(dto.FotoUrl);
        if (!string.IsNullOrWhiteSpace(dto.Senha))
            entidade.AtualizarSenha(_passwordHasher.Hash(dto.Senha));

        await _repositorio.Atualizar(entidade);
    }

    public async Task AtualizarSenha(Guid id, AtualizarSenhaDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Fiscal '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Fiscal '{id}' nao encontrado.");

        if (!_passwordHasher.Verificar(dto.SenhaAtual, entidade.SenhaHash))
            throw new InvalidOperationException("Senha atual incorreta.");

        entidade.AtualizarSenha(_passwordHasher.Hash(dto.NovaSenha));
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Fiscal '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Fiscal '{id}' nao encontrado.");

        await _repositorio.Remover(entidade.Id);
    }

    private static FiscalDto ParaDto(Fiscal e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Usuario = e.Usuario
    };
}
