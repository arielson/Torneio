using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Payments;

public class PaymentLimitsResponse
{
    [JsonProperty("maxValue")]
    public decimal? MaxValue { get; set; }

    [JsonProperty("minValue")]
    public decimal? MinValue { get; set; }

    [JsonProperty("dailyLimit")]
    public LimitInfo? DailyLimit { get; set; }

    [JsonProperty("monthlyLimit")]
    public LimitInfo MonthlyLimit { get; set; } = null!;
}

public class LimitInfo
{
    [JsonProperty("maxValue")]
    public decimal? MaxValue { get; set; }

    [JsonProperty("usedValue")]
    public decimal? UsedValue { get; set; }

    [JsonProperty("availableValue")]
    public decimal? AvailableValue { get; set; }
}
