using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ItemRepositorio : RepositorioBase<Item>, IItemRepositorio
{
    public ItemRepositorio(TorneioDbContext context) : base(context) { }

    public override async Task<Item?> ObterPorId(Guid id) =>
        await _dbSet.Include(i => i.Especie)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Item>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Include(i => i.Especie)
            .IgnoreQueryFilters()
            .Where(i => i.TorneioId == torneioId)
            .OrderBy(i => i.Especie.Nome)
            .ToListAsync();
}
