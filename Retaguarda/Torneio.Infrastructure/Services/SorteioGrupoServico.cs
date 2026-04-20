using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Infrastructure.Services;

public class SorteioGrupoServico : ISorteioGrupoServico
{
    private readonly ISorteioGrupoRepositorio _repositorio;
    private static readonly Random _random = new();

    public SorteioGrupoServico(ISorteioGrupoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<IEnumerable<SorteioGrupo>> RealizarSorteioAsync(
        Guid torneioId,
        IEnumerable<Grupo> grupos,
        IEnumerable<Equipe> equipes)
    {
        var listaGrupos = grupos.ToList();
        // Embaralha as equipes (embarcações) — cada grupo sorteia uma posição
        var equipesEmbaralhadas = equipes.OrderBy(_ => _random.Next()).ToList();

        var resultado = new List<SorteioGrupo>();
        for (int i = 0; i < listaGrupos.Count; i++)
        {
            resultado.Add(SorteioGrupo.Criar(
                torneioId,
                listaGrupos[i].Id,
                equipesEmbaralhadas[i].Id,
                posicao: i + 1));
        }

        return Task.FromResult<IEnumerable<SorteioGrupo>>(resultado);
    }

    public async Task SalvarSorteioAsync(IEnumerable<SorteioGrupo> resultado) =>
        await _repositorio.AdicionarLote(resultado);

    public async Task LimparSorteioAsync(Guid torneioId) =>
        await _repositorio.RemoverPorTorneio(torneioId);
}
