namespace Torneio.Infrastructure.Services.Options;

public class StorageOptions
{
    public const string Section = "Storage";
    public string BasePath { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
