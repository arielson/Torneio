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

    public async Task<IEnumerable<SorteioEquipe>> RealizarSorteioAsync(Guid torneioId)
    {
        var equipes  = (await _equipeRepositorio.ListarTodos()).ToList();
        var membros  = (await _membroRepositorio.ListarTodos()).ToList();
        var resultado = new List<SorteioEquipe>();
        var random   = new Random();

        var membrosEmbaralhados = membros.OrderBy(_ => random.Next()).ToList();

        // Posição global crescente (1, 2, 3... por todos os membros sorteados)
        int posicaoGlobal = 1;
        int idx = 0;
        foreach (var equipe in equipes)
        {
            for (int v = 0; v < equipe.QtdVagas && idx < membrosEmbaralhados.Count; v++)
            {
                var membro = membrosEmbaralhados[idx++];
                resultado.Add(SorteioEquipe.Criar(torneioId, equipe.Id, membro.Id, posicaoGlobal++));
            }
        }

        return resultado;
    }

    public async Task SalvarSorteioAsync(IEnumerable<SorteioEquipe> resultado) =>
        await _sorteioRepositorio.AdicionarLote(resultado);

    public async Task<IEnumerable<SorteioEquipe>> ObterResultadoAsync(Guid torneioId) =>
        await _sorteioRepositorio.ListarPorTorneio(torneioId);

    public async Task LimparSorteioAsync(Guid torneioId) =>
        await _sorteioRepositorio.RemoverPorTorneio(torneioId);
}
