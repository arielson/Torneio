using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class GrupoRepositorio : RepositorioBase<Grupo>, IGrupoRepositorio
{
    public GrupoRepositorio(TorneioDbContext context) : base(context) { }

    public async Task<IEnumerable<Grupo>> ListarComMembros() =>
        await _dbSet
            .Include(g => g.Membros)
                .ThenInclude(m => m.Membro)
            .ToListAsync();

    public async Task<Grupo?> ObterComMembros(Guid id) =>
        await _dbSet
            .Include(g => g.Membros)
                .ThenInclude(m => m.Membro)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<bool> MembroJaEmGrupo(Guid membroId, Guid? excluirGrupoId = null)
    {
        var query = _context.Set<GrupoMembro>()
            .Where(gm => gm.MembroId == membroId);

        if (excluirGrupoId.HasValue)
            query = query.Where(gm => gm.GrupoId != excluirGrupoId.Value);

        // Filtra apenas grupos do tenant corrente via join
        return await query
            .Join(_dbSet, gm => gm.GrupoId, g => g.Id, (gm, g) => gm)
            .AnyAsync();
    }

    public async Task AdicionarMembro(GrupoMembro membro)
    {
        await _context.Set<GrupoMembro>().AddAsync(membro);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverMembro(Guid grupoMembroId)
    {
        var item = await _context.Set<GrupoMembro>().FindAsync(grupoMembroId);
        if (item is not null)
        {
            _context.Set<GrupoMembro>().Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
