using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ICustoTorneioRepositorio : IRepositorio<CustoTorneio>
{
    Task<IEnumerable<CustoTorneio>> ListarPorTorneio(Guid torneioId);
}
