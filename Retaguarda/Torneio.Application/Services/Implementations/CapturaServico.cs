using FluentValidation;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class CapturaServico : ICapturaServico
{
    private readonly ICapturaRepositorio _repositorio;
    private readonly IItemRepositorio _itemRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IValidator<RegistrarCapturaDto> _validador;

    public CapturaServico(
        ICapturaRepositorio repositorio,
        IItemRepositorio itemRepositorio,
        IMembroRepositorio membroRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IValidator<RegistrarCapturaDto> validador)
    {
        _repositorio = repositorio;
        _itemRepositorio = itemRepositorio;
        _membroRepositorio = membroRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _validador = validador;
    }

    public async Task<CapturaDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : await ParaDtoComDetalhes(entidade);
    }

    public async Task<IEnumerable<CapturaDto>> ListarPorEquipe(Guid equipeId)
    {
        var lista = await _repositorio.ListarPorEquipe(equipeId);
        return await ParaDtoListaComDetalhes(lista);
    }

    public async Task<IEnumerable<CapturaDto>> ListarPorMembro(Guid membroId)
    {
        var lista = await _repositorio.ListarPorMembro(membroId);
        return await ParaDtoListaComDetalhes(lista);
    }

    public async Task<IEnumerable<CapturaDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarTodos();
        return await ParaDtoListaComDetalhes(lista);
    }

    public async Task<CapturaDto> Registrar(RegistrarCapturaDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var entidade = Captura.Criar(
            dto.TorneioId,
            dto.ItemId, dto.MembroId, dto.EquipeId,
            dto.TamanhoMedida, dto.FotoUrl, dto.DataHora, dto.PendenteSync);

        await _repositorio.Adicionar(entidade);
        return await ParaDtoComDetalhes(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Captura '{id}' não encontrada.");
        await _repositorio.Remover(entidade.Id);
    }

    public async Task<int> SincronizarLote(IEnumerable<RegistrarCapturaDto> capturas)
    {
        var count = 0;
        foreach (var dto in capturas)
        {
            await _validador.ValidateAndThrowAsync(dto);
            var entidade = Captura.Criar(
                dto.TorneioId,
                dto.ItemId, dto.MembroId, dto.EquipeId,
                dto.TamanhoMedida, dto.FotoUrl, dto.DataHora, pendenteSync: false);
            await _repositorio.Adicionar(entidade);
            count++;
        }
        return count;
    }

    private async Task<CapturaDto> ParaDtoComDetalhes(Captura c)
    {
        var item = await _itemRepositorio.ObterPorId(c.ItemId);
        var membro = await _membroRepositorio.ObterPorId(c.MembroId);
        var equipe = await _equipeRepositorio.ObterPorId(c.EquipeId);

        var fator = item?.FatorMultiplicador ?? 1m;
        var pontuacao = c.TamanhoMedida * fator;

        return new CapturaDto
        {
            Id = c.Id,
            TorneioId = c.TorneioId,
            ItemId = c.ItemId,
            NomeItem = item?.Nome ?? string.Empty,
            MembroId = c.MembroId,
            NomeMembro = membro?.Nome ?? string.Empty,
            EquipeId = c.EquipeId,
            NomeEquipe = equipe?.Nome ?? string.Empty,
            TamanhoMedida = c.TamanhoMedida,
            FatorMultiplicador = fator,
            Pontuacao = pontuacao,
            FotoUrl = c.FotoUrl,
            DataHora = c.DataHora,
            PendenteSync = c.PendenteSync
        };
    }

    private async Task<IEnumerable<CapturaDto>> ParaDtoListaComDetalhes(IEnumerable<Captura> lista)
    {
        var result = new List<CapturaDto>();
        foreach (var c in lista)
            result.Add(await ParaDtoComDetalhes(c));
        return result;
    }
}
