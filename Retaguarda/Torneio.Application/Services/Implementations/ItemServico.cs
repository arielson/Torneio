using FluentValidation;
using Torneio.Application.DTOs.Item;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class ItemServico : IItemServico
{
    private readonly IItemRepositorio _repositorio;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<CriarItemDto> _validador;

    public ItemServico(
        IItemRepositorio repositorio,
        ITenantContext tenantContext,
        IValidator<CriarItemDto> validador)
    {
        _repositorio = repositorio;
        _tenantContext = tenantContext;
        _validador = validador;
    }

    public async Task<ItemDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        if (entidade is null || entidade.TorneioId != _tenantContext.TorneioId)
            return null;

        return ParaDto(entidade);
    }

    public async Task<IEnumerable<ItemDto>> ListarPorTorneio(Guid torneioId)
    {
        var lista = await _repositorio.ListarPorTorneio(torneioId);
        return lista.Select(ParaDto);
    }

    public async Task<ItemDto> Criar(CriarItemDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var entidade = Item.Criar(dto.TorneioId, dto.EspeciePeixeId, dto.Comprimento, dto.FatorMultiplicador);
        await _repositorio.Adicionar(entidade);

        // Reload with navigation property
        var comEspecie = await _repositorio.ObterPorId(entidade.Id);
        return ParaDto(comEspecie!);
    }

    public async Task Atualizar(Guid id, AtualizarItemDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Item '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Item '{id}' nao encontrado.");

        entidade.Atualizar(dto.EspeciePeixeId, dto.Comprimento, dto.FatorMultiplicador);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Item '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Item '{id}' nao encontrado.");

        await _repositorio.Remover(entidade.Id);
    }

    private static ItemDto ParaDto(Item e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        EspeciePeixeId = e.EspeciePeixeId,
        Nome = e.Especie?.Nome ?? string.Empty,
        NomeCientifico = e.Especie?.NomeCientifico,
        FotoUrl = e.Especie?.FotoUrl,
        Comprimento = e.Comprimento,
        FatorMultiplicador = e.FatorMultiplicador
    };
}
