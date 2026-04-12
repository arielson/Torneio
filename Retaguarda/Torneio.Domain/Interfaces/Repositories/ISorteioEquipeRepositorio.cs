using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ISorteioEquipeRepositorio : IRepositorio<SorteioEquipe>
{
    Task<IEnumerable<SorteioEquipe>> ListarPorAnoTorneio(Guid anoTorneioId);
    Task RemoverPorAnoTorneio(Guid anoTorneioId);
}
