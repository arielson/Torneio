using Torneio.Asaas.Models.Accounts;

namespace Torneio.Asaas.Interfaces;

public interface IMyAccountService
{
    Task<MyAccountResponse?> GetCommercialInfoAsync(CancellationToken cancellationToken = default);
    Task<MyAccountResponse?> UpdateCommercialInfoAsync(CommercialInfoRequest request, CancellationToken cancellationToken = default);
    Task<AccountNumberResponse?> GetAccountNumberAsync(CancellationToken cancellationToken = default);
    Task<FeesResponse?> GetFeesAsync(CancellationToken cancellationToken = default);
    Task<AccountStatusResponse?> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<MyAccountResponse?> GetWalletIdAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string removeReason, CancellationToken cancellationToken = default);
}
