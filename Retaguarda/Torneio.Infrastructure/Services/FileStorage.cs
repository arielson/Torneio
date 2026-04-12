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
}
