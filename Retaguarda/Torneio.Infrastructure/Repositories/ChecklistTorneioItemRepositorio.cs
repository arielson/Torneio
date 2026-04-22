using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ChecklistTorneioItemRepositorio : RepositorioBase<ChecklistTorneioItem>, IChecklistTorneioItemRepositorio
{
    public ChecklistTorneioItemRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<ChecklistTorneioItem>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.Concluido)
            .ThenBy(x => x.Data)
            .ThenBy(x => x.Item)
            .ToListAsync();
}
