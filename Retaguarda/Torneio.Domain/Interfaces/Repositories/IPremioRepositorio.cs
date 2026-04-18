using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IPremioRepositorio : IRepositorio<Premio>
{
    Task<IEnumerable<Premio>> ListarPorTorneio(Guid torneioId);
    Task RemoverPorTorneio(Guid torneioId);
}
