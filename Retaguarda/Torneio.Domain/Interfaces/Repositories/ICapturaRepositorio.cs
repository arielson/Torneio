using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ICapturaRepositorio : IRepositorio<Captura>
{
    Task<IEnumerable<Captura>> ListarPorEquipe(Guid equipeId);
    Task<IEnumerable<Captura>> ListarPorMembro(Guid membroId);
    Task<IEnumerable<Captura>> ListarPendenteSync(Guid torneioId);
    Task<IEnumerable<Captura>> ListarTodos();
    Task<IEnumerable<Captura>> ListarPorTorneio(Guid torneioId);
}
