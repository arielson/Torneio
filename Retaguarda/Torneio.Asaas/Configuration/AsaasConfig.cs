namespace Torneio.Asaas.Configuration;

public class AsaasConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.asaas.com";
    public bool IsSandbox { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
    /// <summary>
    /// Optional path prefix prepended to every endpoint.
    /// Production uses "" (empty); sandbox uses "/api" because its base path is /api/v3/.
    /// </summary>
    public string ApiPath { get; set; } = string.Empty;
}
