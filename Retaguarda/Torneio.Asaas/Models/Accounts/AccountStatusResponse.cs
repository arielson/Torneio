using Newtonsoft.Json;

namespace Torneio.Asaas.Models.Accounts;

public class AccountStatusResponse
{
    [JsonProperty("commercialInfoApproved")]
    public bool CommercialInfoApproved { get; set; }

    [JsonProperty("generalApprovalAnalysisType")]
    public string GeneralApprovalAnalysisType { get; set; } = null!;

    [JsonProperty("generalApprovalStatus")]
    public string GeneralApprovalStatus { get; set; } = null!;

    [JsonProperty("bankAccountInfoApprovalStatus")]
    public string BankAccountInfoApprovalStatus { get; set; } = null!;

    [JsonProperty("documentationApprovalStatus")]
    public string DocumentationApprovalStatus { get; set; } = null!;

    [JsonProperty("revenueRangeDeclarationRequired")]
    public bool RevenueRangeDeclarationRequired { get; set; }
}
