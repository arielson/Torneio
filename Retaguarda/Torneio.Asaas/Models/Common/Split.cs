using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class Split
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("walletId")]
    public string WalletId { get; set; } = null!;

    [JsonProperty("fixedValue")]
    public decimal? FixedValue { get; set; }

    [JsonProperty("percentualValue")]
    public decimal? PercentualValue { get; set; }

    [JsonProperty("totalFixedValue")]
    public decimal? TotalFixedValue { get; set; }

    [JsonProperty("totalValue")]
    public decimal? TotalValue { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("cancellationReason")]
    public string CancellationReason { get; set; } = null!;

    [JsonProperty("externalReference")]
    public string ExternalReference { get; set; } = null!;

    [JsonProperty("description")]
    public string Description { get; set; } = null!;
}
