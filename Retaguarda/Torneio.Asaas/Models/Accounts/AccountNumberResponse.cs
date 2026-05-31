using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Accounts;

public class AccountNumberResponse
{
    [JsonProperty("agency")]
    public string Agency { get; set; } = null!;

    [JsonProperty("account")]
    public string Account { get; set; } = null!;

    [JsonProperty("accountDigit")]
    public string AccountDigit { get; set; } = null!;
}
