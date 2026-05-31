using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentBillingInfoResponse
{
    [JsonProperty("pix")]
    public PixInfo? Pix { get; set; }

    [JsonProperty("creditCard")]
    public CreditCardBillingInfo? CreditCard { get; set; }

    [JsonProperty("bankSlip")]
    public BankSlipInfo BankSlip { get; set; } = null!;
}

public class PixInfo
{
    [JsonProperty("encodedImage")]
    public string EncodedImage { get; set; } = null!;

    [JsonProperty("payload")]
    public string Payload { get; set; } = null!;

    [JsonProperty("expirationDate")]
    public string ExpirationDate { get; set; } = null!;
}

public class CreditCardBillingInfo
{
    [JsonProperty("creditCardNumber")]
    public string CreditCardNumber { get; set; } = null!;

    [JsonProperty("creditCardBrand")]
    public string CreditCardBrand { get; set; } = null!;

    [JsonProperty("creditCardToken")]
    public string CreditCardToken { get; set; } = null!;
}

public class BankSlipInfo
{
    [JsonProperty("identificationField")]
    public string IdentificationField { get; set; } = null!;

    [JsonProperty("nossoNumero")]
    public string NossoNumero { get; set; } = null!;

    [JsonProperty("barCode")]
    public string BarCode { get; set; } = null!;

    [JsonProperty("bankSlipUrl")]
    public string BankSlipUrl { get; set; } = null!;

    [JsonProperty("daysAfterDueDateToRegistrationCancellation")]
    public int? DaysAfterDueDateToRegistrationCancellation { get; set; }
}
