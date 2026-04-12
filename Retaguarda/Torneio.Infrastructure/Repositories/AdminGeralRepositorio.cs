using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class AdminGeralRepositorio : RepositorioBase<AdminGeral>, IAdminGeralRepositorio
{
    public AdminGeralRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<AdminGeral?> ObterPorUsuario(string usuario) =>
        await _dbSet.FirstOrDefaultAsync(a => a.Usuario == usuario);
}
