using FluentValidation;
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

    public MembroServico(
        IMembroRepositorio repositorio,
        ITenantContext tenantContext,
        IValidator<CriarMembroDto> validador)
    {
        _repositorio = repositorio;
        _tenantContext = tenantContext;
        _validador = validador;
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
        return lista.Select(ParaDto);
    }

    public async Task<MembroDto> Criar(CriarMembroDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var entidade = Membro.Criar(dto.TorneioId, dto.Nome, dto.FotoUrl);
        await _repositorio.Adicionar(entidade);
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

        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Membro '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Membro '{id}' nao encontrado.");

        await _repositorio.Remover(entidade.Id);
    }

    private static MembroDto ParaDto(Membro e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl
    };
}
