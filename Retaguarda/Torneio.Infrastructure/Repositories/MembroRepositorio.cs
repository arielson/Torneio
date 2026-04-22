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

    public async Task<Membro?> ObterPorCelularNormalizado(Guid torneioId, string celularNormalizado)
    {
        var membros = await _dbSet.IgnoreQueryFilters()
            .Where(m => m.TorneioId == torneioId && m.Celular != null && m.Celular != string.Empty)
            .ToListAsync();

        return membros.FirstOrDefault(m =>
            NormalizarCelular(m.Celular) == celularNormalizado);
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
