using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class MembroRepositorio : RepositorioBase<Membro>, IMembroRepositorio
{
    public MembroRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Membro>> ListarTodos() =>
        await _dbSet.ToListAsync();

    public async Task<IEnumerable<Membro>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(m => m.TorneioId == torneioId)
            .ToListAsync();

    public async Task<IEnumerable<Membro>> ListarPorEquipe(Guid equipeId) =>
        await _context.Equipes
            .Where(e => e.Id == equipeId)
            .SelectMany(e => e.Membros)
            .ToListAsync();
}
