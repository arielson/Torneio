using Torneio.Asaas.Models.Common;
using Torneio.Asaas.Models.Customers;

namespace Torneio.Asaas.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponse?> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default);
    Task<AsaasListResponse<CustomerResponse>?> ListAsync(CustomerListRequest? request = null, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> UpdateAsync(string id, CustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> RestoreAsync(string id, CancellationToken cancellationToken = default);
    Task<CustomerNotificationResponse?> GetNotificationsAsync(string id, CancellationToken cancellationToken = default);
}
