using Torneio.Asaas.Models.Common;
using Torneio.Asaas.Models.Webhooks;

namespace Torneio.Asaas.Interfaces;

public interface IWebhookService
{
    Task<WebhookResponse?> CreateAsync(WebhookRequest request, CancellationToken cancellationToken = default);
    Task<AsaasListResponse<WebhookResponse>?> ListAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    Task<WebhookResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<WebhookResponse?> UpdateAsync(string id, WebhookRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
