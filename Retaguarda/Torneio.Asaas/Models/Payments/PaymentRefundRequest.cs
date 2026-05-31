using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentRefundRequest
{
    [JsonProperty("value")]
    public decimal? Value { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = null!;
}
