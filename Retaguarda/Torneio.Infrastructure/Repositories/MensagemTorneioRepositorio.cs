using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class MensagemTorneioRepositorio : RepositorioBase<MensagemTorneio>, IMensagemTorneioRepositorio
{
    public MensagemTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<MensagemTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(m => m.TorneioId == torneioId)
            .OrderByDescending(m => m.CriadoEm)
            .ToListAsync();
}
