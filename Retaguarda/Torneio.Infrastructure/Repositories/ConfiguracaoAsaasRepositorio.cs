using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class ConfiguracaoAsaasRepositorio : IConfiguracaoAsaasRepositorio
{
    private readonly TorneioDbContext _context;

    public ConfiguracaoAsaasRepositorio(TorneioDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracaoAsaasTorneio?> ObterPorTorneioId(Guid torneioId) =>
        await _context.ConfiguracoesAsaas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TorneioId == torneioId);

    public async Task Adicionar(ConfiguracaoAsaasTorneio config)
    {
        await _context.ConfiguracoesAsaas.AddAsync(config);
        await _context.SaveChangesAsync();
    }

    public async Task Atualizar(ConfiguracaoAsaasTorneio config)
    {
        _context.ConfiguracoesAsaas.Update(config);
        await _context.SaveChangesAsync();
    }
}
