using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IMembroRepositorio : IRepositorio<Membro>
{
    Task<IEnumerable<Membro>> ListarPorAnoTorneio(Guid anoTorneioId);
    Task<IEnumerable<Membro>> ListarPorEquipe(Guid equipeId);
}
