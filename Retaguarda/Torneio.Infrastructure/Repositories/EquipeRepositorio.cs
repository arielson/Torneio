using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class EquipeRepositorio : RepositorioBase<Equipe>, IEquipeRepositorio
{
    public EquipeRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Equipe>> ListarTodos() =>
        await _dbSet
            .Include(e => e.Membros)
            .Include(e => e.Fiscais)
            .ToListAsync();

    public async Task<IEnumerable<Equipe>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Include(e => e.Membros)
            .Include(e => e.Fiscais)
            .Where(e => e.TorneioId == torneioId)
            .ToListAsync();

    public async Task<IEnumerable<Equipe>> ListarPorFiscal(Guid torneioId, Guid fiscalId) =>
        await _dbSet.IgnoreQueryFilters()
            .Include(e => e.Membros)
            .Include(e => e.Fiscais)
            .Where(e => e.TorneioId == torneioId && e.Fiscais.Any(f => f.FiscalId == fiscalId))
            .ToListAsync();

    public async Task<Equipe?> ObterComMembros(Guid id) =>
        await _dbSet
            .Include(e => e.Membros)
            .Include(e => e.Fiscais)
            .FirstOrDefaultAsync(e => e.Id == id);
}
