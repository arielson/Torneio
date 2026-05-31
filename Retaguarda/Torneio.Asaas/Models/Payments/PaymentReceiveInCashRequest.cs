using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentReceiveInCashRequest
{
    [JsonProperty("paymentDate")]
    public string PaymentDate { get; set; } = null!;

    [JsonProperty("value")]
    public decimal? Value { get; set; }

    [JsonProperty("notifyCustomer")]
    public bool? NotifyCustomer { get; set; }
}
