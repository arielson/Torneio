using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentSimulateRequest
{
    [JsonProperty("creditCard")]
    public CreditCardSimulationInfo? CreditCard { get; set; }

    [JsonProperty("creditCardHolderInfo")]
    public CreditCardHolderInfo? CreditCardHolderInfo { get; set; }

    [JsonProperty("creditCardToken")]
    public string CreditCardToken { get; set; } = null!;
}

public class CreditCardSimulationInfo
{
    [JsonProperty("holderName")]
    public string HolderName { get; set; } = null!;

    [JsonProperty("number")]
    public string Number { get; set; } = null!;

    [JsonProperty("expiryMonth")]
    public string ExpiryMonth { get; set; } = null!;

    [JsonProperty("expiryYear")]
    public string ExpiryYear { get; set; } = null!;

    [JsonProperty("ccv")]
    public string Ccv { get; set; } = null!;
}
