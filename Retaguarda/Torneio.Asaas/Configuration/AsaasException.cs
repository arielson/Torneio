using System.Net;

namespace Torneio.Asaas.Configuration;

public class AsaasException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseContent { get; }

    public AsaasException(HttpStatusCode statusCode, string responseContent)
        : base($"Asaas API Error: {statusCode} - {responseContent}")
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}
