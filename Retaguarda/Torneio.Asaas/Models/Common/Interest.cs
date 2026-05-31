using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class Interest
{
    [JsonProperty("value")]
    public decimal Value { get; set; }
}
