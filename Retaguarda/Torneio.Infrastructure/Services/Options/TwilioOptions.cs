namespace Torneio.Infrastructure.Services.Options;

public class TwilioOptions
{
    public const string Section = "Twilio";

    public bool Enabled { get; set; }
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string VerifyServiceSid { get; set; } = string.Empty;
}
