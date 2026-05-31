using Microsoft.EntityFrameworkCore;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Repositories;

public class WebhookEventoAsaasRepositorio : IWebhookEventoAsaasRepositorio
{
    private readonly TorneioDbContext _context;

    public WebhookEventoAsaasRepositorio(TorneioDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAsync(string eventoId) =>
        await _context.WebhookEventosAsaas
            .AnyAsync(e => e.EventoId == eventoId);

    public async Task Adicionar(WebhookEventoAsaas evento)
    {
        await _context.WebhookEventosAsaas.AddAsync(evento);
        await _context.SaveChangesAsync();
    }

    public async Task Atualizar(WebhookEventoAsaas evento)
    {
        _context.WebhookEventosAsaas.Update(evento);
        await _context.SaveChangesAsync();
    }
}
