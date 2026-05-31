using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Webhooks;

public class WebhookRequest
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("url")]
    public string Url { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Include)]
    public bool Enabled { get; set; }

    [JsonProperty("interrupted", NullValueHandling = NullValueHandling.Include)]
    public bool Interrupted { get; set; }

    [JsonProperty("authToken")]
    public string AuthToken { get; set; } = null!;

    [JsonProperty("apiVersion")]
    public int? ApiVersion { get; set; }

    [JsonProperty("sendType")]
    public string SendType { get; set; } = null!;

    [JsonProperty("events")]
    public List<string>? Events { get; set; }
}
