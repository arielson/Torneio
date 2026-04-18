using Torneio.Application.DTOs.Premio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class PremioServico : IPremioServico
{
    private readonly IPremioRepositorio _repositorio;

    public PremioServico(IPremioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<PremioDto>> ListarPorTorneio(Guid torneioId)
    {
        var lista = await _repositorio.ListarPorTorneio(torneioId);
        return lista.OrderBy(p => p.Posicao).Select(ParaDto);
    }

    public async Task<PremioDto> Criar(Guid torneioId, CriarPremioDto dto)
    {
        var existente = (await _repositorio.ListarPorTorneio(torneioId))
            .FirstOrDefault(p => p.Posicao == dto.Posicao);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um prêmio para a posição {dto.Posicao} neste torneio.");

        var entidade = Premio.Criar(torneioId, dto.Posicao, dto.Descricao);
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, string descricao)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Prêmio '{id}' não encontrado.");
        entidade.AtualizarDescricao(descricao);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        _ = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Prêmio '{id}' não encontrado.");
        await _repositorio.Remover(id);
    }

    private static PremioDto ParaDto(Premio e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Posicao = e.Posicao,
        Descricao = e.Descricao,
    };
}
