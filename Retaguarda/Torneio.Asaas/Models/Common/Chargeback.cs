using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class Chargeback
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("payment")]
    public string Payment { get; set; } = null!;

    [JsonProperty("installment")]
    public string Installment { get; set; } = null!;

    [JsonProperty("customerAccount")]
    public string CustomerAccount { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("reason")]
    public string Reason { get; set; } = null!;

    [JsonProperty("disputeStartDate")]
    public string DisputeStartDate { get; set; } = null!;

    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("paymentDate")]
    public string PaymentDate { get; set; } = null!;

    [JsonProperty("creditCard")]
    public CreditCardInfo CreditCard { get; set; } = null!;

    [JsonProperty("disputeStatus")]
    public string DisputeStatus { get; set; } = null!;

    [JsonProperty("deadlineToSendDisputeDocuments")]
    public string DeadlineToSendDisputeDocuments { get; set; } = null!;
}
