using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class MembroRepositorio : RepositorioBase<Membro>, IMembroRepositorio
{
    public MembroRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Membro>> ListarPorAnoTorneio(Guid anoTorneioId) =>
        await _dbSet.Where(m => m.AnoTorneioId == anoTorneioId).ToListAsync();

    public async Task<IEnumerable<Membro>> ListarPorEquipe(Guid equipeId) =>
        await _context.Equipes
            .Where(e => e.Id == equipeId)
            .SelectMany(e => e.Membros)
            .ToListAsync();
}
