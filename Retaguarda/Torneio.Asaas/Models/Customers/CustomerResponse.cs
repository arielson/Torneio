using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Customers;

public class CustomerResponse
{
    [JsonProperty("object")]
    public string Object { get; set; } = null!;

    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("dateCreated")]
    public string DateCreated { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("company")]
    public string Company { get; set; } = null!;

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

    [JsonProperty("personType")]
    public string PersonType { get; set; } = null!;

    [JsonProperty("deleted")]
    public bool Deleted { get; set; }

    [JsonProperty("additionalEmails")]
    public string AdditionalEmails { get; set; } = null!;

    [JsonProperty("externalReference")]
    public string ExternalReference { get; set; } = null!;

    [JsonProperty("notificationDisabled")]
    public bool NotificationDisabled { get; set; }

    [JsonProperty("observations")]
    public string Observations { get; set; } = null!;

    [JsonProperty("municipalInscription")]
    public string MunicipalInscription { get; set; } = null!;

    [JsonProperty("stateInscription")]
    public string StateInscription { get; set; } = null!;

    [JsonProperty("groupName")]
    public string GroupName { get; set; } = null!;

    [JsonProperty("companyType")]
    public string CompanyType { get; set; } = null!;

    [JsonProperty("city")]
    public string City { get; set; } = null!;

    [JsonProperty("state")]
    public string State { get; set; } = null!;

    [JsonProperty("country")]
    public string Country { get; set; } = null!;
}
