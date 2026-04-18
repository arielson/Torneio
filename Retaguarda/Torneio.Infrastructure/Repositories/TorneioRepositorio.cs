using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class TorneioRepositorio : RepositorioBase<TorneioEntity>, ITorneioRepositorio
{
    public TorneioRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<TorneioEntity?> ObterPorSlug(string slug) =>
        await _dbSet.FirstOrDefaultAsync(t => t.Slug == slug);

    public async Task<IEnumerable<TorneioEntity>> ListarAtivos() =>
        await _dbSet.Where(t => t.Ativo).ToListAsync();

    public async Task<IEnumerable<TorneioEntity>> ListarRecentes(int limite) =>
        await _dbSet.Where(t => t.Ativo)
            .OrderByDescending(t => t.CriadoEm)
            .Take(limite)
            .ToListAsync();

    public async Task<IEnumerable<TorneioEntity>> BuscarPorTexto(string q) =>
        await _dbSet.Where(t => t.Ativo &&
            (EF.Functions.ILike(t.NomeTorneio, $"%{q}%") ||
             EF.Functions.ILike(t.Slug, $"%{q}%")))
            .OrderByDescending(t => t.CriadoEm)
            .Take(20)
            .ToListAsync();
}
