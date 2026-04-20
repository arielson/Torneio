using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Services;

public interface ISorteioGrupoServico
{
    /// <summary>Embaralha grupos e equipes, atribuindo cada grupo a uma equipe aleatória. Não persiste.</summary>
    Task<IEnumerable<SorteioGrupo>> RealizarSorteioAsync(
        Guid torneioId,
        IEnumerable<Grupo> grupos,
        IEnumerable<Equipe> equipes);

    Task SalvarSorteioAsync(IEnumerable<SorteioGrupo> resultado);
    Task LimparSorteioAsync(Guid torneioId);
}
