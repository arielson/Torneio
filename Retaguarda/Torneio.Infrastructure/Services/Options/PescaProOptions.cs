namespace Torneio.Infrastructure.Services.Options;

public class PescaProOptions
{
    public const string Section = "PescaPro";

    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
