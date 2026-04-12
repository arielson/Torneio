using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IItemRepositorio : IRepositorio<Item>
{
    Task<IEnumerable<Item>> ListarPorTorneio(Guid torneioId);
}
