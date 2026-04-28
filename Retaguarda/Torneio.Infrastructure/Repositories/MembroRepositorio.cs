using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class MembroRepositorio : RepositorioBase<Membro>, IMembroRepositorio
{
    public MembroRepositorio(TorneioDbContext context) : base(context) { }

    public override async Task<IEnumerable<Membro>> ListarTodos() =>
        await _dbSet.ToListAsync();

    public async Task<IEnumerable<Membro>> ListarPorTorneio(Guid torneioId) =>
        await _dbSet.IgnoreQueryFilters()
            .Where(m => m.TorneioId == torneioId)
            .ToListAsync();

    public async Task<IEnumerable<Membro>> ListarPorEquipe(Guid equipeId) =>
        await _context.Equipes
            .Where(e => e.Id == equipeId)
            .SelectMany(e => e.Membros)
            .ToListAsync();

    public async Task<IEnumerable<Membro>> ListarPorEquipes(IEnumerable<Guid> equipeIds)
    {
        var ids = equipeIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await _context.Equipes
            .IgnoreQueryFilters()
            .Where(e => ids.Contains(e.Id))
            .SelectMany(e => e.Membros)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Membro?> ObterPorCelularNormalizado(Guid torneioId, string celularNormalizado)
    {
        var membros = await _dbSet.IgnoreQueryFilters()
            .Where(m => m.TorneioId == torneioId && m.Celular != null && m.Celular != string.Empty)
            .ToListAsync();

        return membros.FirstOrDefault(m =>
            NormalizarCelular(m.Celular) == celularNormalizado);
    }

    public async Task<(int total, List<string> fotosParaRemover)> RemoverTodos(Guid torneioId)
    {
        var membros = await _dbSet.Where(m => m.TorneioId == torneioId).ToListAsync();
        if (membros.Count == 0) return (0, []);

        var membroIds = membros.Select(m => m.Id).ToHashSet();
        var capturas = await _context.Capturas
            .Where(c => c.TorneioId == torneioId && membroIds.Contains(c.MembroId))
            .ToListAsync();

        var fotos = membros
            .Select(m => m.FotoUrl)
            .Concat(capturas.Select(c => c.FotoUrl))
            .Where(f => !string.IsNullOrWhiteSpace(f) && !f!.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            .Cast<string>()
            .ToList();

        if (capturas.Count > 0)
            _context.Capturas.RemoveRange(capturas);

        _dbSet.RemoveRange(membros);
        await _context.SaveChangesAsync();
        return (membros.Count, fotos);
    }

    public async Task<Membro?> ObterPorUsuario(Guid torneioId, string usuario) =>
        await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m =>
                m.TorneioId == torneioId &&
                m.Usuario != null &&
                m.Usuario.ToLower() == usuario.Trim().ToLower());

    private static string? NormalizarCelular(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var digitos = new string(valor.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digitos))
            return null;

        if (digitos.Length == 10 || digitos.Length == 11)
            return $"+55{digitos}";

        if (digitos.Length == 12 || digitos.Length == 13)
        {
            if (digitos.StartsWith("55"))
                return $"+{digitos}";

            return $"+{digitos}";
        }

        if (valor.TrimStart().StartsWith("+"))
            return $"+{digitos}";

        return $"+{digitos}";
    }
}
