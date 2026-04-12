using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ITorneioRepositorio : IRepositorio<TorneioEntity>
{
    Task<TorneioEntity?> ObterPorSlug(string slug);
    Task<IEnumerable<TorneioEntity>> ListarAtivos();
}
