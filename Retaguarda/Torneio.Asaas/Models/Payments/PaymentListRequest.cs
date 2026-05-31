using Torneio.Asaas.Enums;

namespace Torneio.Asaas.Models.Payments;

public class PaymentListRequest
{
    public string Installment { get; set; } = null!;
    public int? Offset { get; set; }
    public int? Limit { get; set; }
    public string Customer { get; set; } = null!;
    public string CustomerGroupName { get; set; } = null!;
    public BillingType? BillingType { get; set; }
    public PaymentStatus? Status { get; set; }
    public string Subscription { get; set; } = null!;
    public string ExternalReference { get; set; } = null!;
    public string PaymentDate { get; set; } = null!;
    public InvoiceStatus? InvoiceStatus { get; set; }
    public string EstimatedCreditDate { get; set; } = null!;
    public string PixQrCodeId { get; set; } = null!;
    public bool? Anticipated { get; set; }
    public bool? Anticipable { get; set; }
    public string DateCreatedGe { get; set; } = null!;
    public string DateCreatedLe { get; set; } = null!;
    public string PaymentDateGe { get; set; } = null!;
    public string PaymentDateLe { get; set; } = null!;
    public string EstimatedCreditDateGe { get; set; } = null!;
    public string EstimatedCreditDateLe { get; set; } = null!;
    public string DueDateGe { get; set; } = null!;
    public string DueDateLe { get; set; } = null!;
    public string User { get; set; } = null!;
}
