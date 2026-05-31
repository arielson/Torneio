using Newtonsoft.Json;
using Torneio.Asaas.Enums;

namespace Torneio.Asaas.Models.Common;

public class Fine
{
    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("type")]
    public FineType? Type { get; set; }
}
