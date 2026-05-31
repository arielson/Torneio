using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Accounts;

public class CommercialInfoRequest
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;

    [JsonProperty("mobilePhone")]
    public string MobilePhone { get; set; } = null!;

    [JsonProperty("site")]
    public string Site { get; set; } = null!;

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; } = null!;

    [JsonProperty("addressNumber")]
    public string AddressNumber { get; set; } = null!;

    [JsonProperty("addressComplement")]
    public string AddressComplement { get; set; } = null!;

    [JsonProperty("province")]
    public string Province { get; set; } = null!;
}
