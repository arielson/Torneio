using Torneio.Asaas.Configuration;
using Torneio.Asaas.Interfaces;
using Torneio.Asaas.Services;

namespace Torneio.Asaas;

/// <summary>
/// Cliente principal da API Asaas — expõe apenas os serviços usados pelo projeto Torneio.
/// Instanciado por IAsaasClientFactory com a chave de API do torneio.
/// </summary>
public class AsaasClient
{
    public IPaymentService Payments { get; }
    public ICustomerService Customers { get; }
    public IWebhookService Webhooks { get; }
    public IMyAccountService MyAccount { get; }

    public AsaasClient(AsaasConfig config)
    {
        var httpClient = new AsaasHttpClient(config);
        Payments = new PaymentService(httpClient);
        Customers = new CustomerService(httpClient);
        Webhooks = new WebhookService(httpClient);
        MyAccount = new MyAccountService(httpClient);
    }

    /// <param name="apiKey">Chave de API do torneio</param>
    /// <param name="isSandbox">Usa sandbox quando true</param>
    public AsaasClient(string apiKey, bool isSandbox = false)
        : this(new AsaasConfig
        {
            ApiKey = apiKey,
            IsSandbox = isSandbox,
            BaseUrl = isSandbox ? "https://sandbox.asaas.com" : "https://api.asaas.com",
            ApiPath = isSandbox ? "/api" : string.Empty
        })
    {
    }
}
