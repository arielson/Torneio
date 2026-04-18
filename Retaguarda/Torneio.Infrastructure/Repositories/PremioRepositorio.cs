using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class PremioRepositorio : RepositorioBase<Premio>, IPremioRepositorio
{
    public PremioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Premio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet
            .Where(p => p.TorneioId == torneioId)
            .OrderBy(p => p.Posicao)
            .ToListAsync();

    public async Task RemoverPorTorneio(Guid torneioId)
    {
        var premios = await _dbSet.Where(p => p.TorneioId == torneioId).ToListAsync();
        _dbSet.RemoveRange(premios);
        await _context.SaveChangesAsync();
    }
}
