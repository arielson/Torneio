using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ICapturaRepositorio : IRepositorio<Captura>
{
    Task<IEnumerable<Captura>> ListarPorEquipe(Guid equipeId, Guid anoTorneioId);
    Task<IEnumerable<Captura>> ListarPorMembro(Guid membroId, Guid anoTorneioId);
    Task<IEnumerable<Captura>> ListarPendenteSync(Guid torneioId);
    Task<IEnumerable<Captura>> ListarPorAnoTorneio(Guid anoTorneioId);
}
