using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class SorteioEquipeRepositorio : RepositorioBase<SorteioEquipe>, ISorteioEquipeRepositorio
{
    public SorteioEquipeRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<SorteioEquipe>> ListarPorAnoTorneio(Guid anoTorneioId) =>
        await _dbSet.Where(s => s.AnoTorneioId == anoTorneioId).OrderBy(s => s.Posicao).ToListAsync();

    public async Task RemoverPorAnoTorneio(Guid anoTorneioId)
    {
        var registros = await _dbSet.Where(s => s.AnoTorneioId == anoTorneioId).ToListAsync();
        if (registros.Count > 0)
        {
            _context.RemoveRange(registros);
            await _context.SaveChangesAsync();
        }
    }
}
