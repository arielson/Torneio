using System.Web;
using Torneio.Asaas.Configuration;
using Torneio.Asaas.Interfaces;
using Torneio.Asaas.Models.Common;
using Torneio.Asaas.Models.Customers;

namespace Torneio.Asaas.Services;

public class CustomerService(AsaasHttpClient httpClient) : ICustomerService
{
    private readonly AsaasHttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private const string BaseEndpoint = "/v3/customers";

    public async Task<CustomerResponse?> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<CustomerRequest, CustomerResponse>(BaseEndpoint, request, cancellationToken);

    public async Task<AsaasListResponse<CustomerResponse>?> ListAsync(CustomerListRequest? request = null, CancellationToken cancellationToken = default)
    {
        var queryString = BuildQueryString(request);
        var endpoint = string.IsNullOrEmpty(queryString) ? BaseEndpoint : $"{BaseEndpoint}?{queryString}";
        return await _httpClient.GetAsync<AsaasListResponse<CustomerResponse>>(endpoint, cancellationToken);
    }

    public async Task<CustomerResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<CustomerResponse>($"{BaseEndpoint}/{id}", cancellationToken);

    public async Task<CustomerResponse?> UpdateAsync(string id, CustomerRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PutAsync<CustomerRequest, CustomerResponse>($"{BaseEndpoint}/{id}", request, cancellationToken);

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.DeleteAsync($"{BaseEndpoint}/{id}", cancellationToken);

    public async Task<CustomerResponse?> RestoreAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<object, CustomerResponse>($"{BaseEndpoint}/{id}/restore", null, cancellationToken);

    public async Task<CustomerNotificationResponse?> GetNotificationsAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<CustomerNotificationResponse>($"{BaseEndpoint}/{id}/notifications", cancellationToken);

    private static string? BuildQueryString(CustomerListRequest? request)
    {
        if (request == null) return string.Empty;

        var query = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrEmpty(request.Name)) query["name"] = request.Name;
        if (!string.IsNullOrEmpty(request.Email)) query["email"] = request.Email;
        if (!string.IsNullOrEmpty(request.CpfCnpj)) query["cpfCnpj"] = request.CpfCnpj;
        if (!string.IsNullOrEmpty(request.GroupName)) query["groupName"] = request.GroupName;
        if (!string.IsNullOrEmpty(request.ExternalReference)) query["externalReference"] = request.ExternalReference;
        if (request.Offset.HasValue) query["offset"] = request.Offset.Value.ToString();
        if (request.Limit.HasValue) query["limit"] = request.Limit.Value.ToString();

        return query.ToString();
    }
}
