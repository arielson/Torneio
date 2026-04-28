using Microsoft.Extensions.Options;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure.Services;

public class FileStorage : IFileStorage
{
    private readonly StorageOptions _options;

    public FileStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SalvarAsync(Stream conteudo, string nomeArquivo, string subpasta)
    {
        var pasta = Path.Combine(_options.BasePath, subpasta);
        Directory.CreateDirectory(pasta);

        var caminho = Path.Combine(pasta, nomeArquivo);
        await using var fs = File.Create(caminho);
        await conteudo.CopyToAsync(fs);

        return Path.Combine(subpasta, nomeArquivo).Replace('\\', '/');
    }

    public Task RemoverAsync(string caminhoRelativo)
    {
        var caminho = Path.Combine(_options.BasePath, caminhoRelativo);
        if (File.Exists(caminho))
            File.Delete(caminho);
        return Task.CompletedTask;
    }

    public string ObterUrlPublica(string caminhoRelativo) =>
        $"{_options.BaseUrl.TrimEnd('/')}/{caminhoRelativo}";

    public Task<string?> CopiarAsync(string? caminhoRelativoOrigem, string subpastaDestino)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativoOrigem)) return Task.FromResult<string?>(null);
        if (caminhoRelativoOrigem.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<string?>(null);

        var origem = Path.Combine(_options.BasePath, caminhoRelativoOrigem);
        if (!File.Exists(origem)) return Task.FromResult<string?>(null);

        var ext = Path.GetExtension(origem).ToLowerInvariant();
        var nomeArquivo = $"{Guid.NewGuid()}{ext}";
        var pasta = Path.Combine(_options.BasePath, subpastaDestino);
        Directory.CreateDirectory(pasta);
        File.Copy(origem, Path.Combine(pasta, nomeArquivo));
        return Task.FromResult<string?>(Path.Combine(subpastaDestino, nomeArquivo).Replace('\\', '/'));
    }

    public string? UrlParaCaminhoRelativo(string? urlPublica)
    {
        if (string.IsNullOrWhiteSpace(urlPublica)) return null;
        var prefixo = _options.BaseUrl.TrimEnd('/') + "/";
        if (!urlPublica.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase)) return null;
        return urlPublica[prefixo.Length..];
    }
}
