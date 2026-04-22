using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ProdutoExtraMembroRepositorio : RepositorioBase<ProdutoExtraMembro>, IProdutoExtraMembroRepositorio
{
    public ProdutoExtraMembroRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<ProdutoExtraMembro>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<ProdutoExtraMembro>> ListarPorProduto(Guid produtoExtraTorneioId) =>
        await _dbSet.Where(x => x.ProdutoExtraTorneioId == produtoExtraTorneioId)
            .OrderBy(x => x.CriadoEm)
            .ToListAsync();

    public async Task<ProdutoExtraMembro?> ObterPorProdutoEMembro(Guid produtoExtraTorneioId, Guid membroId) =>
        await _dbSet.FirstOrDefaultAsync(x => x.ProdutoExtraTorneioId == produtoExtraTorneioId && x.MembroId == membroId);
}
