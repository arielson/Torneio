using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IValorParcelaTorneioRepositorio : IRepositorio<ValorParcelaTorneio>
{
    Task<IEnumerable<ValorParcelaTorneio>> ListarPorTorneio(Guid torneioId);
    Task RemoverPorTorneio(Guid torneioId);
}
