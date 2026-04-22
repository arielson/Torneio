using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class RegistroPublicoMembroRepositorio : RepositorioBase<RegistroPublicoMembro>, IRegistroPublicoMembroRepositorio
{
    public RegistroPublicoMembroRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<RegistroPublicoMembro?> ObterUltimoPorCelular(Guid torneioId, string celularNormalizado) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(x => x.TorneioId == torneioId && x.CelularNormalizado == celularNormalizado)
            .OrderByDescending(x => x.CriadoEm)
            .FirstOrDefaultAsync();
}
