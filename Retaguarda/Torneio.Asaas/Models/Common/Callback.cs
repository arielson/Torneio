using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Common;

public class Callback
{
    [JsonProperty("successUrl")]
    public string SuccessUrl { get; set; } = null!;

    [JsonProperty("autoRedirect")]
    public bool? AutoRedirect { get; set; }
}
