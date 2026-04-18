using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class CapturaRepositorio : RepositorioBase<Captura>, ICapturaRepositorio
{
    public CapturaRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Captura>> ListarPorEquipe(Guid equipeId) =>
        await _dbSet
            .Where(c => c.EquipeId == equipeId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Captura>> ListarPorMembro(Guid membroId) =>
        await _dbSet
            .Where(c => c.MembroId == membroId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Captura>> ListarPendenteSync(Guid torneioId) =>
        await _dbSet
            .Where(c => c.TorneioId == torneioId && c.PendenteSync)
            .ToListAsync();

    public async Task<IEnumerable<Captura>> ListarTodos() =>
        await _dbSet
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();
}
