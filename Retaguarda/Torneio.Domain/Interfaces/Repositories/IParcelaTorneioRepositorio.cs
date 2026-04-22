using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IParcelaTorneioRepositorio : IRepositorio<ParcelaTorneio>
{
    Task<IEnumerable<ParcelaTorneio>> ListarPorTorneio(Guid torneioId);
    Task<IEnumerable<ParcelaTorneio>> ListarPorMembro(Guid membroId);
    Task RemoverRange(IEnumerable<ParcelaTorneio> parcelas);
}
