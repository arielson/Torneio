using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Services;

public interface ISorteioServico
{
    Task<IEnumerable<SorteioEquipe>> RealizarSorteioAsync(
        Guid torneioId,
        IEnumerable<Equipe> equipes,
        IEnumerable<Membro> membros);
    Task SalvarSorteioAsync(IEnumerable<SorteioEquipe> resultado);
    Task<IEnumerable<SorteioEquipe>> ObterResultadoAsync(Guid torneioId);
    Task LimparSorteioAsync(Guid torneioId);
}
