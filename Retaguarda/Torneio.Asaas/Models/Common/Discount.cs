using Newtonsoft.Json;
using Torneio.Asaas.Enums;

namespace Torneio.Asaas.Models.Common;

public class Discount
{
    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("dueDateLimitDays")]
    public int? DueDateLimitDays { get; set; }

    [JsonProperty("type")]
    public DiscountType? Type { get; set; }
}
