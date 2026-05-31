using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Webhooks;

public class AsaasWebhookPayload
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("event")]
    public string Event { get; set; } = null!;

    [JsonProperty("dateCreated")]
    public string? DateCreated { get; set; }

    [JsonProperty("payment")]
    public AsaasWebhookPaymentData? Payment { get; set; }
}

public class AsaasWebhookPaymentData
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("customer")]
    public string? Customer { get; set; }

    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("netValue")]
    public decimal NetValue { get; set; }

    [JsonProperty("billingType")]
    public string? BillingType { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("dueDate")]
    public string? DueDate { get; set; }

    [JsonProperty("paymentDate")]
    public string? PaymentDate { get; set; }

    [JsonProperty("clientPaymentDate")]
    public string? ClientPaymentDate { get; set; }

    [JsonProperty("creditDate")]
    public string? CreditDate { get; set; }

    [JsonProperty("estimatedCreditDate")]
    public string? EstimatedCreditDate { get; set; }

    [JsonProperty("externalReference")]
    public string? ExternalReference { get; set; }
}
