using FluentValidation;
using Torneio.Application.DTOs.Item;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class ItemServico : IItemServico
{
    private readonly IItemRepositorio _repositorio;
    private readonly IValidator<CriarItemDto> _validador;

    public ItemServico(IItemRepositorio repositorio, IValidator<CriarItemDto> validador)
    {
        _repositorio = repositorio;
        _validador = validador;
    }

    public async Task<ItemDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<ItemDto>> ListarPorTorneio(Guid torneioId)
    {
        var lista = await _repositorio.ListarPorTorneio(torneioId);
        return lista.Select(ParaDto);
    }

    public async Task<ItemDto> Criar(CriarItemDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var entidade = Item.Criar(dto.TorneioId, dto.Nome, dto.Comprimento, dto.FatorMultiplicador, dto.FotoUrl);
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarItemDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Item '{id}' não encontrado.");

        entidade.Atualizar(dto.Nome, dto.Comprimento, dto.FatorMultiplicador, dto.FotoUrl);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Item '{id}' não encontrado.");
        await _repositorio.Remover(entidade.Id);
    }

    private static ItemDto ParaDto(Item e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Comprimento = e.Comprimento,
        FatorMultiplicador = e.FatorMultiplicador
    };
}
