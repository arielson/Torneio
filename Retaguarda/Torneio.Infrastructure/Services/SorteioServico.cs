using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Infrastructure.Services;

public class SorteioServico : ISorteioServico
{
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ISorteioEquipeRepositorio _sorteioRepositorio;

    public SorteioServico(
        IEquipeRepositorio equipeRepositorio,
        IMembroRepositorio membroRepositorio,
        ISorteioEquipeRepositorio sorteioRepositorio)
    {
        _equipeRepositorio = equipeRepositorio;
        _membroRepositorio = membroRepositorio;
        _sorteioRepositorio = sorteioRepositorio;
    }

    public async Task<IEnumerable<SorteioEquipe>> RealizarSorteioAsync(Guid torneioId, Guid anoTorneioId)
    {
        var equipes = (await _equipeRepositorio.ListarPorAnoTorneio(anoTorneioId)).ToList();
        var resultado = new List<SorteioEquipe>();
        var random = new Random();

        // Embaralha equipes aleatoriamente
        var embaralhadas = equipes.OrderBy(_ => random.Next()).ToList();

        for (int i = 0; i < embaralhadas.Count; i++)
        {
            var equipe = embaralhadas[i];
            var membros = (await _membroRepositorio.ListarPorEquipe(equipe.Id)).ToList();
            if (membros.Count == 0) continue;

            var membroSorteado = membros[random.Next(membros.Count)];
            var registro = SorteioEquipe.Criar(torneioId, anoTorneioId, equipe.Id, membroSorteado.Id, i + 1);

            await _sorteioRepositorio.Adicionar(registro);
            resultado.Add(registro);
        }

        return resultado;
    }

    public async Task<IEnumerable<SorteioEquipe>> ObterResultadoAsync(Guid torneioId, Guid anoTorneioId) =>
        await _sorteioRepositorio.ListarPorAnoTorneio(anoTorneioId);

    public async Task LimparSorteioAsync(Guid torneioId, Guid anoTorneioId) =>
        await _sorteioRepositorio.RemoverPorAnoTorneio(anoTorneioId);
}
