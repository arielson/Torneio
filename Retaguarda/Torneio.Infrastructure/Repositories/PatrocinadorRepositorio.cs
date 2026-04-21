using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class PatrocinadorRepositorio : RepositorioBase<Patrocinador>, IPatrocinadorRepositorio
{
    public PatrocinadorRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Patrocinador>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(p => p.TorneioId == torneioId)
            .OrderBy(p => p.Nome)
            .ToListAsync();
}
