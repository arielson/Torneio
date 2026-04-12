using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Services;

public interface ISorteioServico
{
    Task<IEnumerable<SorteioEquipe>> RealizarSorteioAsync(Guid torneioId, Guid anoTorneioId);
    Task<IEnumerable<SorteioEquipe>> ObterResultadoAsync(Guid torneioId, Guid anoTorneioId);
    Task LimparSorteioAsync(Guid torneioId, Guid anoTorneioId);
}
