using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class BannerRepositorio : RepositorioBase<Banner>, IBannerRepositorio
{
    public BannerRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Banner>> ListarAtivos() =>
        await _dbSet.Include(b => b.Torneio)
            .Where(b => b.Ativo)
            .OrderBy(b => b.Ordem)
            .ToListAsync();

    public override async Task<IEnumerable<Banner>> ListarTodos() =>
        await _dbSet.Include(b => b.Torneio)
            .OrderBy(b => b.Ordem)
            .ToListAsync();

    public async Task<Banner?> ObterComTorneio(Guid id) =>
        await _dbSet.Include(b => b.Torneio).FirstOrDefaultAsync(b => b.Id == id);
}
