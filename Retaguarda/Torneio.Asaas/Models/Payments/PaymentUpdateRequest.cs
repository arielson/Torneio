using Newtonsoft.Json;
using Torneio.Asaas.Enums;
using Torneio.Asaas.Models.Common;

namespace Torneio.Asaas.Models.Payments;

public class PaymentUpdateRequest
{
    [JsonProperty("billingType")]
    public BillingType? BillingType { get; set; }

    [JsonProperty("value")]
    public decimal? Value { get; set; }

    [JsonProperty("dueDate")]
    public string DueDate { get; set; } = null!;

    [JsonProperty("description")]
    public string Description { get; set; } = null!;

    [JsonProperty("daysAfterDueDateToRegistrationCancellation")]
    public int? DaysAfterDueDateToRegistrationCancellation { get; set; }

    [JsonProperty("externalReference")]
    public string ExternalReference { get; set; } = null!;

    [JsonProperty("discount")]
    public Discount? Discount { get; set; }

    [JsonProperty("interest")]
    public Interest? Interest { get; set; }

    [JsonProperty("fine")]
    public Fine? Fine { get; set; }

    [JsonProperty("postalService")]
    public bool? PostalService { get; set; }

    [JsonProperty("split")]
    public List<Split>? Split { get; set; }

    [JsonProperty("callback")]
    public Callback Callback { get; set; } = null!;
}
