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
    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly ITenantContext _tenantContext;

    public SorteioAppServico(
        ISorteioServico sorteioServico,
        ISorteioEquipeRepositorio sorteioRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IMembroRepositorio membroRepositorio,
        ITorneioRepositorio torneioRepositorio,
        ITenantContext tenantContext)
    {
        _sorteioServico = sorteioServico;
        _sorteioRepositorio = sorteioRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _membroRepositorio = membroRepositorio;
        _torneioRepositorio = torneioRepositorio;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio()
    {
        var torneio = await _torneioRepositorio.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException($"Torneio '{_tenantContext.TorneioId}' não encontrado.");

        if (torneio.Status != StatusTorneio.Liberado)
            throw new InvalidOperationException("O sorteio só pode ser realizado em torneios com status Liberado.");

        await _sorteioServico.LimparSorteioAsync(torneio.Id);
        var resultado = await _sorteioServico.RealizarSorteioAsync(torneio.Id);
        return await ParaDtoLista(resultado);
    }

    public async Task<IEnumerable<SorteioEquipeDto>> ObterResultado()
    {
        var resultado = await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId);
        return await ParaDtoLista(resultado);
    }

    public async Task AjustarPosicao(Guid sorteioEquipeId, int novaPosicao)
    {
        var entidade = await _sorteioRepositorio.ObterPorId(sorteioEquipeId)
            ?? throw new KeyNotFoundException($"SorteioEquipe '{sorteioEquipeId}' não encontrado.");
        entidade.AjustarPosicao(novaPosicao);
        await _sorteioRepositorio.Atualizar(entidade);
    }

    public async Task LimparSorteio()
    {
        await _sorteioServico.LimparSorteioAsync(_tenantContext.TorneioId);
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
