using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class AsaasListResponse<T>
{
    [JsonProperty("object")]
    public string Object { get; set; } = null!;

    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }

    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }

    [JsonProperty("limit")]
    public int Limit { get; set; }

    [JsonProperty("offset")]
    public int Offset { get; set; }

    [JsonProperty("data")]
    public List<T> Data { get; set; } = null!;
}
