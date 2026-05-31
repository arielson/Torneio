using System.Web;
using Torneio.Asaas.Configuration;
using Torneio.Asaas.Interfaces;
using Torneio.Asaas.Models.Common;
using Torneio.Asaas.Models.Webhooks;

namespace Torneio.Asaas.Services;

public class WebhookService(AsaasHttpClient httpClient) : IWebhookService
{
    private readonly AsaasHttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private const string BaseEndpoint = "/v3/webhooks";

    public async Task<WebhookResponse?> CreateAsync(WebhookRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<WebhookRequest, WebhookResponse>(BaseEndpoint, request, cancellationToken);

    public async Task<AsaasListResponse<WebhookResponse>?> ListAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        if (offset.HasValue) query["offset"] = offset.Value.ToString();
        if (limit.HasValue) query["limit"] = limit.Value.ToString();

        var queryString = query.ToString();
        var endpoint = string.IsNullOrEmpty(queryString) ? BaseEndpoint : $"{BaseEndpoint}?{queryString}";
        return await _httpClient.GetAsync<AsaasListResponse<WebhookResponse>>(endpoint, cancellationToken);
    }

    public async Task<WebhookResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<WebhookResponse>($"{BaseEndpoint}/{id}", cancellationToken);

    public async Task<WebhookResponse?> UpdateAsync(string id, WebhookRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PutAsync<WebhookRequest, WebhookResponse>($"{BaseEndpoint}/{id}", request, cancellationToken);

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.DeleteAsync($"{BaseEndpoint}/{id}", cancellationToken);
}
