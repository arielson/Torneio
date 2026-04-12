using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public abstract class RepositorioBase<T> : IRepositorio<T> where T : class
{
    protected readonly TorneioDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected RepositorioBase(TorneioDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorId(Guid id) =>
        await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> ListarTodos() =>
        await _dbSet.ToListAsync();

    public virtual async Task Adicionar(T entidade)
    {
        await _dbSet.AddAsync(entidade);
        await _context.SaveChangesAsync();
    }

    public virtual async Task Atualizar(T entidade)
    {
        _dbSet.Update(entidade);
        await _context.SaveChangesAsync();
    }

    public virtual async Task Remover(Guid id)
    {
        var entidade = await ObterPorId(id);
        if (entidade is not null)
        {
            _dbSet.Remove(entidade);
            await _context.SaveChangesAsync();
        }
    }
}
