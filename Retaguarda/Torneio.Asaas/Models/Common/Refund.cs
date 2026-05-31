using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class Refund
{
    [JsonProperty("dateCreated")]
    public string DateCreated { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("endToEndIdentifier")]
    public string EndToEndIdentifier { get; set; } = null!;

    [JsonProperty("description")]
    public string Description { get; set; } = null!;

    [JsonProperty("effectiveDate")]
    public string EffectiveDate { get; set; } = null!;

    [JsonProperty("transactionReceiptUrl")]
    public string TransactionReceiptUrl { get; set; } = null!;

    [JsonProperty("refundedSplits")]
    public List<RefundedSplit> RefundedSplits { get; set; } = null!;
}

public class RefundedSplit
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("done")]
    public bool Done { get; set; }
}
