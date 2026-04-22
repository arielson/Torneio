using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class ChecklistTorneioItemServico : IChecklistTorneioItemServico
{
    private readonly IChecklistTorneioItemRepositorio _repositorio;
    private readonly ITenantContext _tenantContext;

    public ChecklistTorneioItemServico(
        IChecklistTorneioItemRepositorio repositorio,
        ITenantContext tenantContext)
    {
        _repositorio = repositorio;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ChecklistTorneioItemDto>> Listar(Guid torneioId) =>
        (await _repositorio.ListarPorTorneio(torneioId)).Select(ParaDto);

    public async Task<ChecklistTorneioItemDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<ChecklistTorneioItemDto> Criar(CriarChecklistTorneioItemDto dto)
    {
        var entidade = ChecklistTorneioItem.Criar(dto.TorneioId, dto.Item, dto.Data, dto.Responsavel, dto.Concluido);
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarChecklistTorneioItemDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Checklist '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Checklist '{id}' nao encontrado.");

        entidade.Atualizar(dto.Item, dto.Data, dto.Responsavel, dto.Concluido);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Checklist '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Checklist '{id}' nao encontrado.");

        await _repositorio.Remover(id);
    }

    private static ChecklistTorneioItemDto ParaDto(ChecklistTorneioItem x) => new()
    {
        Id = x.Id,
        TorneioId = x.TorneioId,
        Item = x.Item,
        Data = x.Data,
        Responsavel = x.Responsavel,
        Concluido = x.Concluido
    };
}
