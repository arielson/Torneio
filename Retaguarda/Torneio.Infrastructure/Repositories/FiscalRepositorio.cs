using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class FiscalRepositorio : RepositorioBase<Fiscal>, IFiscalRepositorio
{
    public FiscalRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<Fiscal?> ObterPorUsuario(string usuario, Guid torneioId) =>
        await _dbSet
            .Include(f => f.Equipes)
            .FirstOrDefaultAsync(f => f.Usuario == usuario && f.TorneioId == torneioId);

    public async Task<IEnumerable<Fiscal>> ListarTodos() =>
        await _dbSet
            .Include(f => f.Equipes)
            .ToListAsync();

    public async Task<IEnumerable<Fiscal>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Include(f => f.Equipes)
            .Where(f => f.TorneioId == torneioId)
            .ToListAsync();

    public async Task<Fiscal?> ObterComEquipes(Guid id) =>
        await _dbSet
            .Include(f => f.Equipes)
            .FirstOrDefaultAsync(f => f.Id == id);
}
