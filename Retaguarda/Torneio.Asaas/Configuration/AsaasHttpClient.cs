using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Torneio.Asaas.Configuration;

public class AsaasHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly AsaasConfig _config;

    public AsaasHttpClient(AsaasConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // Disable auto-redirect to preserve HTTP method and custom headers (e.g. access_token).
        // .NET's default redirect changes POST→GET and strips custom headers on 302.
        var handler = new HttpClientHandler { AllowAutoRedirect = false };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds)
        };

        _httpClient.DefaultRequestHeaders.Add("access_token", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Torneio/1.0");
    }

    private string Ep(string endpoint) => _config.ApiPath + endpoint;

    private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
    };

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await SendWithRedirectsAsync(HttpMethod.Get, Ep(endpoint), null, cancellationToken);
        return await ProcessResponse<TResponse>(response);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
    {
        var json = JsonConvert.SerializeObject(data, _serializerSettings);
        var response = await SendWithRedirectsAsync(HttpMethod.Post, Ep(endpoint), json, cancellationToken);
        return await ProcessResponse<TResponse>(response);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        var json = JsonConvert.SerializeObject(data, _serializerSettings);
        var response = await SendWithRedirectsAsync(HttpMethod.Put, Ep(endpoint), json, cancellationToken);
        return await ProcessResponse<TResponse>(response);
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await SendWithRedirectsAsync(HttpMethod.Delete, Ep(endpoint), null, cancellationToken);
        return await ProcessResponse<TResponse>(response);
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await SendWithRedirectsAsync(HttpMethod.Delete, Ep(endpoint), null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task<HttpResponseMessage> SendWithRedirectsAsync(
        HttpMethod method, string url, string? body, CancellationToken cancellationToken)
    {
        for (int i = 0; i < 10; i++)
        {
            var request = new HttpRequestMessage(method, url);
            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found
                or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {
                var location = response.Headers.Location;
                if (location == null) return response;
                url = location.IsAbsoluteUri
                    ? location.ToString()
                    : new Uri(_httpClient.BaseAddress!, location).ToString();
                continue;
            }

            return response;
        }

        throw new Exception("Too many redirects from Asaas API.");
    }

    private async Task<TResponse?> ProcessResponse<TResponse>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new AsaasException(response.StatusCode, content);

        if (string.IsNullOrWhiteSpace(content))
            return default;

        return JsonConvert.DeserializeObject<TResponse>(content);
    }
}
