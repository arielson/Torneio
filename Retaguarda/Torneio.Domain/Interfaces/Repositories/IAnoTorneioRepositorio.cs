using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IAnoTorneioRepositorio : IRepositorio<AnoTorneio>
{
    Task<AnoTorneio?> ObterPorAno(Guid torneioId, int ano);
    Task<IEnumerable<AnoTorneio>> ListarPorTorneio(Guid torneioId);
    Task<AnoTorneio?> ObterUltimoAno(Guid torneioId);
}
