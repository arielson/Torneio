using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentStatusResponse
{
    [JsonProperty("status")]
    public string Status { get; set; } = null!;
}
