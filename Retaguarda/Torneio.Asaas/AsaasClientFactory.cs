using Microsoft.Extensions.Options;

namespace Torneio.Asaas;

public class AsaasClientFactory : IAsaasClientFactory
{
    private readonly bool _isSandbox;

    public AsaasClientFactory(IOptions<AsaasOptions> options)
    {
        _isSandbox = options.Value.IsSandbox;
    }

    public AsaasClient Criar(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Chave de API do Asaas não pode ser vazia.", nameof(apiKey));

        return new AsaasClient(apiKey, _isSandbox);
    }
}
