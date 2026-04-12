using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class AnoTorneioRepositorio : RepositorioBase<AnoTorneio>, IAnoTorneioRepositorio
{
    public AnoTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<AnoTorneio?> ObterPorAno(Guid torneioId, int ano) =>
        await _dbSet.FirstOrDefaultAsync(a => a.TorneioId == torneioId && a.Ano == ano);

    public async Task<IEnumerable<AnoTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(a => a.TorneioId == torneioId).OrderByDescending(a => a.Ano).ToListAsync();

    public async Task<AnoTorneio?> ObterUltimoAno(Guid torneioId) =>
        await _dbSet.Where(a => a.TorneioId == torneioId).OrderByDescending(a => a.Ano).FirstOrDefaultAsync();
}
