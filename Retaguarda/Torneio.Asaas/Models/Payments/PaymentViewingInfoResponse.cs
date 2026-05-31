using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentViewingInfoResponse
{
    [JsonProperty("url")]
    public string Url { get; set; } = null!;

    [JsonProperty("expirationDate")]
    public string ExpirationDate { get; set; } = null!;
}
