using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class EspeciePeixeRepositorio : RepositorioBase<EspeciePeixe>, IEspeciePeixeRepositorio
{
    public EspeciePeixeRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<EspeciePeixe>> ListarTodas() =>
        await _dbSet.OrderBy(e => e.Nome).ToListAsync();
}
