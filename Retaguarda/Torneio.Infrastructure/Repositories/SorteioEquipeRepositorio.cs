using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class SorteioEquipeRepositorio : RepositorioBase<SorteioEquipe>, ISorteioEquipeRepositorio
{
    public SorteioEquipeRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<SorteioEquipe>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(s => s.TorneioId == torneioId).OrderBy(s => s.Posicao).ToListAsync();

    public async Task AdicionarLote(IEnumerable<SorteioEquipe> lista)
    {
        await _dbSet.AddRangeAsync(lista);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverPorTorneio(Guid torneioId)
    {
        var registros = await _dbSet.Where(s => s.TorneioId == torneioId).ToListAsync();
        if (registros.Count > 0)
        {
            _context.RemoveRange(registros);
            await _context.SaveChangesAsync();
        }
    }
}
