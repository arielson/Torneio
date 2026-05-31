using System.Web;
using Torneio.Asaas.Configuration;
using Torneio.Asaas.Interfaces;
using Torneio.Asaas.Models.Accounts;

namespace Torneio.Asaas.Services;

public class MyAccountService(AsaasHttpClient httpClient) : IMyAccountService
{
    private readonly AsaasHttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private const string BaseEndpoint = "/v3/myAccount";

    public async Task<MyAccountResponse?> GetCommercialInfoAsync(CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<MyAccountResponse>($"{BaseEndpoint}/commercialInfo/", cancellationToken);

    public async Task<MyAccountResponse?> UpdateCommercialInfoAsync(CommercialInfoRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PutAsync<CommercialInfoRequest, MyAccountResponse>($"{BaseEndpoint}/commercialInfo/", request, cancellationToken);

    public async Task<AccountNumberResponse?> GetAccountNumberAsync(CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<AccountNumberResponse>($"{BaseEndpoint}/accountNumber", cancellationToken);

    public async Task<FeesResponse?> GetFeesAsync(CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<FeesResponse>($"{BaseEndpoint}/fees/", cancellationToken);

    public async Task<AccountStatusResponse?> GetStatusAsync(CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<AccountStatusResponse>($"{BaseEndpoint}/status/", cancellationToken);

    public async Task<MyAccountResponse?> GetWalletIdAsync(CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<MyAccountResponse>($"{BaseEndpoint}/", cancellationToken);

    public async Task<bool> DeleteAsync(string removeReason, CancellationToken cancellationToken = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["removeReason"] = removeReason;
        return await _httpClient.DeleteAsync($"{BaseEndpoint}/?{query}", cancellationToken);
    }
}
