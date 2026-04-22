using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ParcelaTorneioRepositorio : RepositorioBase<ParcelaTorneio>, IParcelaTorneioRepositorio
{
    public ParcelaTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<ParcelaTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderBy(x => x.NumeroParcela)
            .ThenBy(x => x.Vencimento)
            .ToListAsync();

    public async Task<IEnumerable<ParcelaTorneio>> ListarPorMembro(Guid membroId) =>
        await _dbSet.Where(x => x.MembroId == membroId)
            .OrderBy(x => x.NumeroParcela)
            .ToListAsync();

    public async Task RemoverRange(IEnumerable<ParcelaTorneio> parcelas)
    {
        _dbSet.RemoveRange(parcelas);
        await _context.SaveChangesAsync();
    }
}
