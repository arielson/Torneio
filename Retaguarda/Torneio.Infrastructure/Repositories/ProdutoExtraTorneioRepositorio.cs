using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ProdutoExtraTorneioRepositorio : RepositorioBase<ProdutoExtraTorneio>, IProdutoExtraTorneioRepositorio
{
    public ProdutoExtraTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<ProdutoExtraTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.Nome)
            .ToListAsync();
}
