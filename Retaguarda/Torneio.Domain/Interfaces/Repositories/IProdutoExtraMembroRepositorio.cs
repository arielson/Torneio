using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IProdutoExtraMembroRepositorio : IRepositorio<ProdutoExtraMembro>
{
    Task<IEnumerable<ProdutoExtraMembro>> ListarPorTorneio(Guid torneioId);
    Task<IEnumerable<ProdutoExtraMembro>> ListarPorProduto(Guid produtoExtraTorneioId);
    Task<ProdutoExtraMembro?> ObterPorProdutoEMembro(Guid produtoExtraTorneioId, Guid membroId);
}
