using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ITorneioRepositorio : IRepositorio<TorneioEntity>
{
    Task<TorneioEntity?> ObterPorSlug(string slug);
    Task<IEnumerable<TorneioEntity>> ListarAtivos();
    Task<IEnumerable<TorneioEntity>> ListarRecentes(int limite);
    Task<IEnumerable<TorneioEntity>> BuscarPorTexto(string q);
}
