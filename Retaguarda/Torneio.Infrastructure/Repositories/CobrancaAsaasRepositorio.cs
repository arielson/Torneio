using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class CobrancaAsaasRepositorio : ICobrancaAsaasRepositorio
{
    private readonly TorneioDbContext _context;

    public CobrancaAsaasRepositorio(TorneioDbContext context)
    {
        _context = context;
    }

    public async Task<CobrancaAsaas?> ObterPorParcelaId(Guid parcelaTorneioId) =>
        await _context.CobrancasAsaas
            .Where(c => c.ParcelaTorneioId == parcelaTorneioId)
            .OrderByDescending(c => c.CriadoEm)
            .FirstOrDefaultAsync();

    public async Task<CobrancaAsaas?> ObterPorAsaasPaymentId(string asaasPaymentId) =>
        await _context.CobrancasAsaas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.AsaasPaymentId == asaasPaymentId);

    public async Task<IEnumerable<CobrancaAsaas>> ListarPorMembro(Guid torneioId, Guid membroId) =>
        await _context.CobrancasAsaas
            .Where(c => c.TorneioId == torneioId && c.MembroId == membroId)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync();

    public async Task Adicionar(CobrancaAsaas cobranca)
    {
        await _context.CobrancasAsaas.AddAsync(cobranca);
        await _context.SaveChangesAsync();
    }

    public async Task Atualizar(CobrancaAsaas cobranca)
    {
        _context.CobrancasAsaas.Update(cobranca);
        await _context.SaveChangesAsync();
    }
}
