using System.Web;
using Torneio.Asaas.Configuration;
using Torneio.Asaas.Interfaces;
using Torneio.Asaas.Models.Common;
using Torneio.Asaas.Models.Payments;

namespace Torneio.Asaas.Services;

public class PaymentService(AsaasHttpClient httpClient) : IPaymentService
{
    private readonly AsaasHttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private const string BaseEndpoint = "/v3/payments";

    public async Task<PaymentResponse?> CreateAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentRequest, PaymentResponse>(BaseEndpoint, request, cancellationToken);

    public async Task<PaymentResponse?> CreateWithCreditCardAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentRequest, PaymentResponse>(BaseEndpoint, request, cancellationToken);

    public async Task<AsaasListResponse<PaymentResponse>?> ListAsync(PaymentListRequest? request = null, CancellationToken cancellationToken = default)
    {
        var queryString = BuildQueryString(request);
        var endpoint = string.IsNullOrEmpty(queryString) ? BaseEndpoint : $"{BaseEndpoint}?{queryString}";
        return await _httpClient.GetAsync<AsaasListResponse<PaymentResponse>>(endpoint, cancellationToken);
    }

    public async Task<PaymentResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PaymentResponse>($"{BaseEndpoint}/{id}", cancellationToken);

    public async Task<PaymentResponse?> UpdateAsync(string id, PaymentUpdateRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PutAsync<PaymentUpdateRequest, PaymentResponse>($"{BaseEndpoint}/{id}", request, cancellationToken);

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.DeleteAsync($"{BaseEndpoint}/{id}", cancellationToken);

    public async Task<PaymentResponse?> RestoreAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<object, PaymentResponse>($"{BaseEndpoint}/{id}/restore", null, cancellationToken);

    public async Task<PaymentResponse?> CaptureAsync(string id, PaymentCaptureRequest? request = null, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentCaptureRequest, PaymentResponse>($"{BaseEndpoint}/{id}/captureAuthorizedPayment", request, cancellationToken);

    public async Task<PaymentResponse?> PayWithCreditCardAsync(string id, PaymentRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentRequest, PaymentResponse>($"{BaseEndpoint}/{id}/payWithCreditCard", request, cancellationToken);

    public async Task<PaymentBillingInfoResponse?> GetBillingInfoAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PaymentBillingInfoResponse>($"{BaseEndpoint}/{id}/billingInfo", cancellationToken);

    public async Task<PaymentViewingInfoResponse?> GetViewingInfoAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PaymentViewingInfoResponse>($"{BaseEndpoint}/{id}/viewingInfo", cancellationToken);

    public async Task<PaymentStatusResponse?> GetStatusAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PaymentStatusResponse>($"{BaseEndpoint}/{id}/status", cancellationToken);

    public async Task<PaymentResponse?> RefundAsync(string id, PaymentRefundRequest? request = null, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentRefundRequest, PaymentResponse>($"{BaseEndpoint}/{id}/refund", request, cancellationToken);

    public async Task<PaymentIdentificationFieldResponse?> GetIdentificationFieldAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PaymentIdentificationFieldResponse>($"{BaseEndpoint}/{id}/identificationField", cancellationToken);

    public async Task<PixQrCodeResponse?> GetPixQrCodeAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PixQrCodeResponse>($"{BaseEndpoint}/{id}/pixQrCode", cancellationToken);

    public async Task<PaymentResponse?> ReceiveInCashAsync(string id, PaymentReceiveInCashRequest? request = null, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentReceiveInCashRequest, PaymentResponse>($"{BaseEndpoint}/{id}/receiveInCash", request, cancellationToken);

    public async Task<PaymentResponse?> UndoReceivedInCashAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<object, PaymentResponse>($"{BaseEndpoint}/{id}/undoReceivedInCash", null, cancellationToken);

    public async Task<PaymentResponse?> SimulateAsync(PaymentSimulateRequest request, CancellationToken cancellationToken = default)
        => await _httpClient.PostAsync<PaymentSimulateRequest, PaymentResponse>($"{BaseEndpoint}/simulate", request, cancellationToken);

    public async Task<PaymentLimitsResponse?> GetLimitsAsync(CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<PaymentLimitsResponse>($"{BaseEndpoint}/limits", cancellationToken);

    public async Task<Escrow?> GetEscrowAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<Escrow>($"{BaseEndpoint}/{id}/escrow", cancellationToken);

    public async Task<AsaasListResponse<Refund>?> ListRefundsAsync(string id, CancellationToken cancellationToken = default)
        => await _httpClient.GetAsync<AsaasListResponse<Refund>>($"{BaseEndpoint}/{id}/refunds", cancellationToken);

    private static string? BuildQueryString(PaymentListRequest? request)
    {
        if (request == null) return string.Empty;

        var query = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrEmpty(request.Installment)) query["installment"] = request.Installment;
        if (request.Offset.HasValue) query["offset"] = request.Offset.Value.ToString();
        if (request.Limit.HasValue) query["limit"] = request.Limit.Value.ToString();
        if (!string.IsNullOrEmpty(request.Customer)) query["customer"] = request.Customer;
        if (!string.IsNullOrEmpty(request.CustomerGroupName)) query["customerGroupName"] = request.CustomerGroupName;
        if (request.BillingType.HasValue) query["billingType"] = request.BillingType.Value.ToString();
        if (request.Status.HasValue) query["status"] = request.Status.Value.ToString();
        if (!string.IsNullOrEmpty(request.Subscription)) query["subscription"] = request.Subscription;
        if (!string.IsNullOrEmpty(request.ExternalReference)) query["externalReference"] = request.ExternalReference;
        if (!string.IsNullOrEmpty(request.PaymentDate)) query["paymentDate"] = request.PaymentDate;
        if (request.InvoiceStatus.HasValue) query["invoiceStatus"] = request.InvoiceStatus.Value.ToString();
        if (!string.IsNullOrEmpty(request.EstimatedCreditDate)) query["estimatedCreditDate"] = request.EstimatedCreditDate;
        if (!string.IsNullOrEmpty(request.PixQrCodeId)) query["pixQrCodeId"] = request.PixQrCodeId;
        if (request.Anticipated.HasValue) query["anticipated"] = request.Anticipated.Value.ToString().ToLower();
        if (request.Anticipable.HasValue) query["anticipable"] = request.Anticipable.Value.ToString().ToLower();
        if (!string.IsNullOrEmpty(request.DateCreatedGe)) query["dateCreated[ge]"] = request.DateCreatedGe;
        if (!string.IsNullOrEmpty(request.DateCreatedLe)) query["dateCreated[le]"] = request.DateCreatedLe;
        if (!string.IsNullOrEmpty(request.PaymentDateGe)) query["paymentDate[ge]"] = request.PaymentDateGe;
        if (!string.IsNullOrEmpty(request.PaymentDateLe)) query["paymentDate[le]"] = request.PaymentDateLe;
        if (!string.IsNullOrEmpty(request.EstimatedCreditDateGe)) query["estimatedCreditDate[ge]"] = request.EstimatedCreditDateGe;
        if (!string.IsNullOrEmpty(request.EstimatedCreditDateLe)) query["estimatedCreditDate[le]"] = request.EstimatedCreditDateLe;
        if (!string.IsNullOrEmpty(request.DueDateGe)) query["dueDate[ge]"] = request.DueDateGe;
        if (!string.IsNullOrEmpty(request.DueDateLe)) query["dueDate[le]"] = request.DueDateLe;
        if (!string.IsNullOrEmpty(request.User)) query["user"] = request.User;

        return query.ToString();
    }
}
