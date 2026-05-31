using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PixQrCodeResponse
{
    [JsonProperty("encodedImage")]
    public string EncodedImage { get; set; } = null!;

    [JsonProperty("payload")]
    public string Payload { get; set; } = null!;

    [JsonProperty("expirationDate")]
    public string ExpirationDate { get; set; } = null!;
}
