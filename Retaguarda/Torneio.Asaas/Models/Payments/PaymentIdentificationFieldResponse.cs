using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentIdentificationFieldResponse
{
    [JsonProperty("identificationField")]
    public string IdentificationField { get; set; } = null!;

    [JsonProperty("nossoNumero")]
    public string NossoNumero { get; set; } = null!;
}
