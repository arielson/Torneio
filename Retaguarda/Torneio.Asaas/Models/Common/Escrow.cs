using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class Escrow
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("expirationDate")]
    public string ExpirationDate { get; set; } = null!;

    [JsonProperty("finishDate")]
    public string FinishDate { get; set; } = null!;

    [JsonProperty("finishReason")]
    public string FinishReason { get; set; } = null!;
}
