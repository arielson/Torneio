using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IMembroRepositorio : IRepositorio<Membro>
{
    Task<IEnumerable<Membro>> ListarTodos();
    Task<IEnumerable<Membro>> ListarPorTorneio(Guid torneioId);
    Task<IEnumerable<Membro>> ListarPorEquipe(Guid equipeId);
}
