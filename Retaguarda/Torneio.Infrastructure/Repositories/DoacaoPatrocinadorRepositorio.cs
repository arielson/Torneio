using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class DoacaoPatrocinadorRepositorio : RepositorioBase<DoacaoPatrocinador>, IDoacaoPatrocinadorRepositorio
{
    public DoacaoPatrocinadorRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<DoacaoPatrocinador>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.Where(x => x.TorneioId == torneioId)
            .OrderByDescending(x => x.DataDoacao)
            .ThenBy(x => x.NomePatrocinador)
            .ToListAsync();
}
