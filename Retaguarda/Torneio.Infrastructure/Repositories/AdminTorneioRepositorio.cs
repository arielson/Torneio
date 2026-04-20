using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class AdminTorneioRepositorio : RepositorioBase<AdminTorneio>, IAdminTorneioRepositorio
{
    public AdminTorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<AdminTorneio?> ObterPorUsuario(string usuario) =>
        await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Usuario == usuario);

    public async Task<AdminTorneio?> ObterPorUsuario(string usuario, Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Usuario == usuario && a.TorneioId == torneioId);

    public async Task<IEnumerable<AdminTorneio>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(a => a.TorneioId == torneioId)
            .ToListAsync();

    public async Task<IEnumerable<AdminTorneio>> ListarPorUsuarioId(Guid usuarioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(a => a.UsuarioId == usuarioId)
            .ToListAsync();
}
