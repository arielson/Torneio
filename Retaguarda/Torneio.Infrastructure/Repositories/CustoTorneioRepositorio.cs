using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class CustoTorneioRepositorio : RepositorioBase<CustoTorneio>, ICustoTorneioRepositorio
{
    public CustoTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<CustoTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.Categoria)
            .ThenBy(x => x.Descricao)
            .ToListAsync();
}
