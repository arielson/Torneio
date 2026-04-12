using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class EquipeRepositorio : RepositorioBase<Equipe>, IEquipeRepositorio
{
    public EquipeRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Equipe>> ListarPorAnoTorneio(Guid anoTorneioId) =>
        await _dbSet.Where(e => e.AnoTorneioId == anoTorneioId).ToListAsync();

    public async Task<Equipe?> ObterPorFiscal(Guid fiscalId, Guid anoTorneioId) =>
        await _dbSet.FirstOrDefaultAsync(e => e.FiscalId == fiscalId && e.AnoTorneioId == anoTorneioId);

    public async Task<Equipe?> ObterComMembros(Guid id) =>
        await _dbSet.Include(e => e.Membros).FirstOrDefaultAsync(e => e.Id == id);
}
