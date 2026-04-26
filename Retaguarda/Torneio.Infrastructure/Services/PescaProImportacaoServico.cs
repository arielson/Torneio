using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure.Services;

public class PescaProImportacaoServico : IPescaProImportacaoServico
{
    private readonly PescaProOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public PescaProImportacaoServico(IOptions<PescaProOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public bool Configurado =>
        !string.IsNullOrWhiteSpace(_options.ApiUrl) && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<IReadOnlyList<PescaProPescadorDto>> ListarPescadores()
    {
        if (!Configurado)
            throw new InvalidOperationException("Integracao PescaPro nao configurada. Defina PescaPro:ApiUrl e PescaPro:ApiKey no appsettings.");

        var client = _httpClientFactory.CreateClient("PescaPro");
        client.DefaultRequestHeaders.Remove("X-API-KEY");
        client.DefaultRequestHeaders.Add("X-API-KEY", _options.ApiKey);

        var url = _options.ApiUrl.TrimEnd('/') + "/api/v1/Pescador/ListarParaImportacao";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<PescaProResponseItem>>();
        if (result is null) return [];

        return result
            .Select(x => new PescaProPescadorDto
            {
                Id = x.Id.ToString(),
                Nome = x.Nome,
                Celular = x.Celular > 0 ? x.Celular.ToString() : null,
                FotoUrl = x.FotoUrl
            })
            .ToList();
    }

    private sealed class PescaProResponseItem
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("nome")]
        public string Nome { get; init; } = null!;

        [JsonPropertyName("celular")]
        public long Celular { get; init; }

        [JsonPropertyName("fotoUrl")]
        public string? FotoUrl { get; init; }
    }
}
