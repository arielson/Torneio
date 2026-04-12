using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class CapturaRepositorio : RepositorioBase<Captura>, ICapturaRepositorio
{
    public CapturaRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Captura>> ListarPorEquipe(Guid equipeId, Guid anoTorneioId) =>
        await _dbSet
            .Where(c => c.EquipeId == equipeId && c.AnoTorneioId == anoTorneioId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Captura>> ListarPorMembro(Guid membroId, Guid anoTorneioId) =>
        await _dbSet
            .Where(c => c.MembroId == membroId && c.AnoTorneioId == anoTorneioId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<Captura>> ListarPendenteSync(Guid torneioId) =>
        await _dbSet
            .Where(c => c.TorneioId == torneioId && c.PendenteSync)
            .ToListAsync();

    public async Task<IEnumerable<Captura>> ListarPorAnoTorneio(Guid anoTorneioId) =>
        await _dbSet
            .Where(c => c.AnoTorneioId == anoTorneioId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();
}
