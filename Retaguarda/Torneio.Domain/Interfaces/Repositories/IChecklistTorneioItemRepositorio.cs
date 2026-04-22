using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IChecklistTorneioItemRepositorio : IRepositorio<ChecklistTorneioItem>
{
    Task<IEnumerable<ChecklistTorneioItem>> ListarPorTorneio(Guid torneioId);
}
