using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Customers;

public class CustomerNotificationResponse
{
    [JsonProperty("customer")]
    public string Customer { get; set; } = null!;

    [JsonProperty("emailEnabledForProvider")]
    public bool EmailEnabledForProvider { get; set; }

    [JsonProperty("smsEnabledForProvider")]
    public bool SmsEnabledForProvider { get; set; }

    [JsonProperty("emailEnabledForCustomer")]
    public bool EmailEnabledForCustomer { get; set; }

    [JsonProperty("smsEnabledForCustomer")]
    public bool SmsEnabledForCustomer { get; set; }

    [JsonProperty("whatsappEnabledForCustomer")]
    public bool WhatsappEnabledForCustomer { get; set; }
}
