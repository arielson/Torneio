using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class LogAuditoriaRepositorio : ILogAuditoriaRepositorio
{
    private readonly TorneioDbContext _context;

    public LogAuditoriaRepositorio(TorneioDbContext context)
        => _context = context;

    public async Task Adicionar(LogAuditoria log)
    {
        await _context.Logs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<int> LimparTodos()
    {
        var logs = await _context.Logs.ToListAsync();
        var total = logs.Count;
        if (total == 0) return 0;

        _context.Logs.RemoveRange(logs);
        await _context.SaveChangesAsync();
        return total;
    }

    public async Task<(IEnumerable<LogAuditoria> Itens, int Total)> Listar(
        Guid? torneioId,
        string? categoria,
        string? usuarioPerfil,
        string? busca,
        DateTime? dataInicio,
        DateTime? dataFim,
        int pagina,
        int tamanhoPagina)
    {
        var query = _context.Logs.AsNoTracking().AsQueryable();

        if (torneioId.HasValue)
            query = query.Where(l => l.TorneioId == torneioId.Value);

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(l => l.Categoria == categoria);

        if (!string.IsNullOrWhiteSpace(usuarioPerfil))
            query = query.Where(l => l.UsuarioPerfil == usuarioPerfil);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(l =>
                l.Descricao.ToLower().Contains(termo) ||
                l.UsuarioNome.ToLower().Contains(termo) ||
                (l.NomeTorneio != null && l.NomeTorneio.ToLower().Contains(termo)));
        }

        if (dataInicio.HasValue)
            query = query.Where(l => l.DataHora >= dataInicio.Value.ToUniversalTime());

        if (dataFim.HasValue)
            query = query.Where(l => l.DataHora <= dataFim.Value.ToUniversalTime().AddDays(1));

        var total = await query.CountAsync();
        var itens = await query
            .OrderByDescending(l => l.DataHora)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }
}
