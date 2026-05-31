using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IWebhookEventoAsaasRepositorio
{
    Task<bool> ExisteAsync(string eventoId);
    Task Adicionar(WebhookEventoAsaas evento);
    Task Atualizar(WebhookEventoAsaas evento);
}
