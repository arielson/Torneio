using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IProdutoExtraTorneioRepositorio : IRepositorio<ProdutoExtraTorneio>
{
    Task<IEnumerable<ProdutoExtraTorneio>> ListarPorTorneio(Guid torneioId);
}
