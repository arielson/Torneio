using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Accounts;

public class FeesResponse
{
    [JsonProperty("transfer")]
    public TransferFees Transfer { get; set; } = null!;

    [JsonProperty("payment")]
    public PaymentFees Payment { get; set; } = null!;
}

public class TransferFees
{
    [JsonProperty("ted")]
    public decimal Ted { get; set; }

    [JsonProperty("pix")]
    public decimal Pix { get; set; }
}

public class PaymentFees
{
    [JsonProperty("bankSlip")]
    public BankSlipFees BankSlip { get; set; } = null!;

    [JsonProperty("creditCard")]
    public CreditCardFees CreditCard { get; set; } = null!;

    [JsonProperty("pix")]
    public decimal Pix { get; set; }
}

public class BankSlipFees
{
    [JsonProperty("defaultValue")]
    public decimal DefaultValue { get; set; }

    [JsonProperty("discountValue")]
    public decimal DiscountValue { get; set; }

    [JsonProperty("discountExpiration")]
    public string DiscountExpiration { get; set; } = null!;
}

public class CreditCardFees
{
    [JsonProperty("oneInstallmentPercentage")]
    public decimal OneInstallmentPercentage { get; set; }

    [JsonProperty("upToSixInstallmentsPercentage")]
    public decimal UpToSixInstallmentsPercentage { get; set; }

    [JsonProperty("upToTwelveInstallmentsPercentage")]
    public decimal UpToTwelveInstallmentsPercentage { get; set; }

    [JsonProperty("discountPercentage")]
    public decimal DiscountPercentage { get; set; }

    [JsonProperty("discountExpiration")]
    public string DiscountExpiration { get; set; } = null!;
}
