namespace Torneio.Application.Services.Interfaces;

public interface IWebhookAsaasProcessador
{
    Task ProcessarAsync(string payloadJson);
}
