using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ValorParcelaTorneioRepositorio : RepositorioBase<ValorParcelaTorneio>, IValorParcelaTorneioRepositorio
{
    public ValorParcelaTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<ValorParcelaTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.NumeroParcela)
            .ToListAsync();

    public async Task RemoverPorTorneio(Guid torneioId)
    {
        var registros = await _dbSet.Where(x => x.TorneioId == torneioId).ToListAsync();
        if (registros.Count > 0)
        {
            _dbSet.RemoveRange(registros);
            await _context.SaveChangesAsync();
        }
    }
}
