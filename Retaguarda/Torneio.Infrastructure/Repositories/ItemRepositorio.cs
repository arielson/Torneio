using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ItemRepositorio : RepositorioBase<Item>, IItemRepositorio
{
    public ItemRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Item>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(i => i.TorneioId == torneioId).ToListAsync();
}
