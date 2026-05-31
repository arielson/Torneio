using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Accounts;

public class MyAccountResponse
{
    [JsonProperty("walletId")]
    public string WalletId { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("loginEmail")]
    public string LoginEmail { get; set; } = null!;

    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;

    [JsonProperty("mobilePhone")]
    public string MobilePhone { get; set; } = null!;

    [JsonProperty("address")]
    public string Address { get; set; } = null!;

    [JsonProperty("addressNumber")]
    public string AddressNumber { get; set; } = null!;

    [JsonProperty("complement")]
    public string Complement { get; set; } = null!;

    [JsonProperty("province")]
    public string Province { get; set; } = null!;

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; } = null!;

    [JsonProperty("cpfCnpj")]
    public string CpfCnpj { get; set; } = null!;

    [JsonProperty("birthDate")]
    public string BirthDate { get; set; } = null!;

    [JsonProperty("personType")]
    public string PersonType { get; set; } = null!;

    [JsonProperty("companyType")]
    public string CompanyType { get; set; } = null!;

    [JsonProperty("city")]
    public string City { get; set; } = null!;

    [JsonProperty("state")]
    public string State { get; set; } = null!;

    [JsonProperty("country")]
    public string Country { get; set; } = null!;
}
