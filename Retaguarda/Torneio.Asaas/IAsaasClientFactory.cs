namespace Torneio.Asaas;

/// <summary>
/// Cria instâncias de AsaasClient com a chave de API do torneio.
/// Cada torneio tem sua própria chave — nunca usar singleton.
/// </summary>
public interface IAsaasClientFactory
{
    AsaasClient Criar(string apiKey);
}
