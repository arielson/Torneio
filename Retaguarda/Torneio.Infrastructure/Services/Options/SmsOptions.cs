namespace Torneio.Infrastructure.Services.Options;

public class SmsOptions
{
    public const string Section = "Sms";

    public string Provedor { get; set; } = "Desabilitado"; // Twilio | KingSms | Desabilitado
    public string MensagemCodigo { get; set; } = "Seu código de acesso: {0}";
    public string NumeroRedirecionamentoDev { get; set; } = string.Empty;

    public TwilioSmsConfig Twilio { get; set; } = new();
    public KingSmsConfig KingSms { get; set; } = new();
}

public class TwilioSmsConfig
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class KingSmsConfig
{
    public string Login { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
