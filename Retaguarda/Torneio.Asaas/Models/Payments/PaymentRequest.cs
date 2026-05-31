using Newtonsoft.Json;
using Torneio.Asaas.Enums;
using Torneio.Asaas.Models.Common;

namespace Torneio.Asaas.Models.Payments;

public class PaymentRequest
{
    [JsonProperty("customer")]
    public string Customer { get; set; } = null!;

    [JsonProperty("billingType")]
    public BillingType BillingType { get; set; }

    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("dueDate")]
    public string DueDate { get; set; } = null!;

    [JsonProperty("description")]
    public string Description { get; set; } = null!;

    [JsonProperty("daysAfterDueDateToRegistrationCancellation")]
    public int? DaysAfterDueDateToRegistrationCancellation { get; set; }

    [JsonProperty("externalReference")]
    public string ExternalReference { get; set; } = null!;

    [JsonProperty("installmentCount")]
    public int? InstallmentCount { get; set; }

    [JsonProperty("totalValue")]
    public decimal? TotalValue { get; set; }

    [JsonProperty("installmentValue")]
    public decimal? InstallmentValue { get; set; }

    [JsonProperty("discount")]
    public Discount? Discount { get; set; }

    [JsonProperty("interest")]
    public Interest? Interest { get; set; }

    [JsonProperty("fine")]
    public Fine? Fine { get; set; }

    [JsonProperty("postalService")]
    public bool? PostalService { get; set; }

    [JsonProperty("split")]
    public List<Split>? Split { get; set; }

    [JsonProperty("callback")]
    public Callback? Callback { get; set; }

    [JsonProperty("creditCard")]
    public CreditCardInfo? CreditCard { get; set; }

    [JsonProperty("creditCardHolderInfo")]
    public CreditCardHolderInfo? CreditCardHolderInfo { get; set; }

    [JsonProperty("creditCardToken")]
    public string CreditCardToken { get; set; } = null!;

    [JsonProperty("remoteIp")]
    public string RemoteIp { get; set; } = null!;

    [JsonProperty("authorizeOnly")]
    public bool? AuthorizeOnly { get; set; }
}

public class CreditCardHolderInfo
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("cpfCnpj")]
    public string CpfCnpj { get; set; } = null!;

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; } = null!;

    [JsonProperty("addressNumber")]
    public string AddressNumber { get; set; } = null!;

    [JsonProperty("addressComplement")]
    public string AddressComplement { get; set; } = null!;

    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;

    [JsonProperty("mobilePhone")]
    public string MobilePhone { get; set; } = null!;
}
