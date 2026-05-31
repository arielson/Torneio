using Newtonsoft.Json;
using Torneio.Asaas.Models.Common;

namespace Torneio.Asaas.Models.Payments;

public class PaymentResponse
{
    [JsonProperty("object")]
    public string Object { get; set; } = null!;

    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("dateCreated")]
    public string DateCreated { get; set; } = null!;

    [JsonProperty("customer")]
    public string Customer { get; set; } = null!;

    [JsonProperty("subscription")]
    public string Subscription { get; set; } = null!;

    [JsonProperty("installment")]
    public string Installment { get; set; } = null!;

    [JsonProperty("paymentLink")]
    public string PaymentLink { get; set; } = null!;

    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("netValue")]
    public decimal NetValue { get; set; }

    [JsonProperty("originalValue")]
    public decimal? OriginalValue { get; set; }

    [JsonProperty("interestValue")]
    public decimal? InterestValue { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = null!;

    [JsonProperty("billingType")]
    public string BillingType { get; set; } = null!;

    [JsonProperty("creditCard")]
    public CreditCardInfo? CreditCard { get; set; }

    [JsonProperty("canBePaidAfterDueDate")]
    public bool CanBePaidAfterDueDate { get; set; }

    [JsonProperty("pixTransaction")]
    public string PixTransaction { get; set; } = null!;

    [JsonProperty("pixQrCodeId")]
    public string PixQrCodeId { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("dueDate")]
    public string DueDate { get; set; } = null!;

    [JsonProperty("originalDueDate")]
    public string OriginalDueDate { get; set; } = null!;

    [JsonProperty("paymentDate")]
    public string PaymentDate { get; set; } = null!;

    [JsonProperty("clientPaymentDate")]
    public string ClientPaymentDate { get; set; } = null!;

    [JsonProperty("installmentNumber")]
    public int? InstallmentNumber { get; set; }

    [JsonProperty("invoiceUrl")]
    public string InvoiceUrl { get; set; } = null!;

    [JsonProperty("invoiceNumber")]
    public string InvoiceNumber { get; set; } = null!;

    [JsonProperty("externalReference")]
    public string ExternalReference { get; set; } = null!;

    [JsonProperty("deleted")]
    public bool Deleted { get; set; }

    [JsonProperty("anticipated")]
    public bool Anticipated { get; set; }

    [JsonProperty("anticipable")]
    public bool Anticipable { get; set; }

    [JsonProperty("creditDate")]
    public string CreditDate { get; set; } = null!;

    [JsonProperty("estimatedCreditDate")]
    public string EstimatedCreditDate { get; set; } = null!;

    [JsonProperty("transactionReceiptUrl")]
    public string TransactionReceiptUrl { get; set; } = null!;

    [JsonProperty("nossoNumero")]
    public string NossoNumero { get; set; } = null!;

    [JsonProperty("bankSlipUrl")]
    public string BankSlipUrl { get; set; } = null!;

    [JsonProperty("discount")]
    public Discount? Discount { get; set; }

    [JsonProperty("fine")]
    public Fine? Fine { get; set; }

    [JsonProperty("interest")]
    public Interest? Interest { get; set; }

    [JsonProperty("split")]
    public List<Split>? Split { get; set; }

    [JsonProperty("postalService")]
    public bool? PostalService { get; set; }

    [JsonProperty("daysAfterDueDateToRegistrationCancellation")]
    public int? DaysAfterDueDateToRegistrationCancellation { get; set; }

    [JsonProperty("chargeback")]
    public Chargeback? Chargeback { get; set; }

    [JsonProperty("escrow")]
    public Escrow? Escrow { get; set; }

    [JsonProperty("refunds")]
    public List<Refund> Refunds { get; set; } = null!;
}
