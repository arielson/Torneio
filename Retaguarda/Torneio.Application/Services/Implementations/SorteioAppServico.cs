using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class SorteioAppServico : ISorteioAppServico
{
    private readonly ISorteioServico _sorteioServico;
    private readonly ISorteioEquipeRepositorio _sorteioRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IAnoTorneioRepositorio _anoTorneioRepositorio;

    public SorteioAppServico(
        ISorteioServico sorteioServico,
        ISorteioEquipeRepositorio sorteioRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IMembroRepositorio membroRepositorio,
        IAnoTorneioRepositorio anoTorneioRepositorio)
    {
        _sorteioServico = sorteioServico;
        _sorteioRepositorio = sorteioRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _membroRepositorio = membroRepositorio;
        _anoTorneioRepositorio = anoTorneioRepositorio;
    }

    public async Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio(Guid anoTorneioId)
    {
        var anoTorneio = await _anoTorneioRepositorio.ObterPorId(anoTorneioId)
            ?? throw new KeyNotFoundException($"Ano do torneio '{anoTorneioId}' não encontrado.");

        if (anoTorneio.Status != StatusAnoTorneio.Liberado)
            throw new InvalidOperationException("O sorteio só pode ser realizado em anos com status Liberado.");

        await _sorteioServico.LimparSorteioAsync(anoTorneio.TorneioId, anoTorneioId);
        var resultado = await _sorteioServico.RealizarSorteioAsync(anoTorneio.TorneioId, anoTorneioId);
        return await ParaDtoLista(resultado);
    }

    public async Task<IEnumerable<SorteioEquipeDto>> ObterResultado(Guid anoTorneioId)
    {
        var resultado = await _sorteioRepositorio.ListarPorAnoTorneio(anoTorneioId);
        return await ParaDtoLista(resultado);
    }

    public async Task AjustarPosicao(Guid sorteioEquipeId, int novaPosicao)
    {
        var entidade = await _sorteioRepositorio.ObterPorId(sorteioEquipeId)
            ?? throw new KeyNotFoundException($"SorteioEquipe '{sorteioEquipeId}' não encontrado.");
        entidade.AjustarPosicao(novaPosicao);
        await _sorteioRepositorio.Atualizar(entidade);
    }

    public async Task LimparSorteio(Guid anoTorneioId)
    {
        var anoTorneio = await _anoTorneioRepositorio.ObterPorId(anoTorneioId)
            ?? throw new KeyNotFoundException($"Ano do torneio '{anoTorneioId}' não encontrado.");
        await _sorteioServico.LimparSorteioAsync(anoTorneio.TorneioId, anoTorneioId);
    }

    private async Task<IEnumerable<SorteioEquipeDto>> ParaDtoLista(IEnumerable<SorteioEquipe> lista)
    {
        var result = new List<SorteioEquipeDto>();
        foreach (var s in lista)
        {
            var equipe = await _equipeRepositorio.ObterPorId(s.EquipeId);
            var membro = await _membroRepositorio.ObterPorId(s.MembroId);
            result.Add(new SorteioEquipeDto
            {
                Id = s.Id,
                EquipeId = s.EquipeId,
                NomeEquipe = equipe?.Nome ?? string.Empty,
                MembroId = s.MembroId,
                NomeMembro = membro?.Nome ?? string.Empty,
                Posicao = s.Posicao
            });
        }
        return result.OrderBy(r => r.Posicao);
    }
}
