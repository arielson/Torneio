using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class TorneioRepositorio : RepositorioBase<TorneioEntity>, ITorneioRepositorio
{
    public TorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<TorneioEntity?> ObterPorSlug(string slug) =>
        await _dbSet.FirstOrDefaultAsync(t => t.Slug == slug);

    public async Task<IEnumerable<TorneioEntity>> ListarAtivos() =>
        await _dbSet.Where(t => t.Ativo).ToListAsync();
}
