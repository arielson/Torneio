using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class SeguidorTorneioRepositorio : RepositorioBase<SeguidorTorneio>, ISeguidorTorneioRepositorio
{
    public SeguidorTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<SeguidorTorneio?> ObterPorToken(Guid torneioId, string deviceToken) =>
        await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TorneioId == torneioId && s.DeviceToken == deviceToken);

    public async Task<IEnumerable<string>> ListarTokens(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(s => s.TorneioId == torneioId)
            .Select(s => s.DeviceToken)
            .ToListAsync();

    public async Task RemoverPorToken(Guid torneioId, string deviceToken)
    {
        var seguidor = await ObterPorToken(torneioId, deviceToken);
        if (seguidor is null) return;
        _dbSet.Remove(seguidor);
        await _context.SaveChangesAsync();
    }
}
