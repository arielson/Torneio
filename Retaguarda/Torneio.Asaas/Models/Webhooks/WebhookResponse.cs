using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Webhooks;

public class WebhookResponse
{
    [JsonProperty("object")]
    public string Object { get; set; } = null!;

    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("url")]
    public string Url { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("apiVersion")]
    public int ApiVersion { get; set; }

    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("interrupted")]
    public bool Interrupted { get; set; }

    [JsonProperty("authToken")]
    public string AuthToken { get; set; } = null!;

    [JsonProperty("sendType")]
    public string SendType { get; set; } = null!;
}
