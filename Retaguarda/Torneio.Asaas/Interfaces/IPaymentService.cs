using Torneio.Asaas.Models.Common;
using Torneio.Asaas.Models.Payments;

namespace Torneio.Asaas.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse?> CreateAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> CreateWithCreditCardAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<AsaasListResponse<PaymentResponse>?> ListAsync(PaymentListRequest? request = null, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> UpdateAsync(string id, PaymentUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> RestoreAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> CaptureAsync(string id, PaymentCaptureRequest? request = null, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> PayWithCreditCardAsync(string id, PaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentBillingInfoResponse?> GetBillingInfoAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentViewingInfoResponse?> GetViewingInfoAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentStatusResponse?> GetStatusAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> RefundAsync(string id, PaymentRefundRequest? request = null, CancellationToken cancellationToken = default);
    Task<PaymentIdentificationFieldResponse?> GetIdentificationFieldAsync(string id, CancellationToken cancellationToken = default);
    Task<PixQrCodeResponse?> GetPixQrCodeAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> ReceiveInCashAsync(string id, PaymentReceiveInCashRequest? request = null, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> UndoReceivedInCashAsync(string id, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> SimulateAsync(PaymentSimulateRequest request, CancellationToken cancellationToken = default);
    Task<PaymentLimitsResponse?> GetLimitsAsync(CancellationToken cancellationToken = default);
    Task<Escrow?> GetEscrowAsync(string id, CancellationToken cancellationToken = default);
    Task<AsaasListResponse<Refund>?> ListRefundsAsync(string id, CancellationToken cancellationToken = default);
}
