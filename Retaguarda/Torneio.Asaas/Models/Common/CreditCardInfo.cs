using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class CreditCardInfo
{
    [JsonProperty("holderName")]
    public string HolderName { get; set; } = null!;

    [JsonProperty("number")]
    public string Number { get; set; } = null!;

    [JsonProperty("creditCardNumber")]
    public string CreditCardNumber { get; set; } = null!;

    [JsonProperty("brand")]
    public string Brand { get; set; } = null!;

    [JsonProperty("creditCardBrand")]
    public string CreditCardBrand { get; set; } = null!;

    [JsonProperty("creditCardToken")]
    public string CreditCardToken { get; set; } = null!;

    [JsonProperty("expiryMonth")]
    public string ExpiryMonth { get; set; } = null!;

    [JsonProperty("expiryYear")]
    public string ExpiryYear { get; set; } = null!;

    [JsonProperty("ccv")]
    public string Ccv { get; set; } = null!;
}
