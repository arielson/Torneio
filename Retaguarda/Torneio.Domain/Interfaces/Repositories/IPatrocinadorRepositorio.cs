using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IPatrocinadorRepositorio : IRepositorio<Patrocinador>
{
    Task<IEnumerable<Patrocinador>> ListarPorTorneio(Guid torneioId);
}
