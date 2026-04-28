namespace Torneio.Domain.Interfaces.Services;

public interface IFileStorage
{
    Task<string> SalvarAsync(Stream conteudo, string nomeArquivo, string subpasta);
    Task RemoverAsync(string caminhoRelativo);
    string ObterUrlPublica(string caminhoRelativo);
    /// <summary>Converte uma URL pública de volta para o caminho relativo no storage. Retorna null se não for uma URL do storage local.</summary>
    string? UrlParaCaminhoRelativo(string? urlPublica);
    /// <summary>Copia um arquivo local para outra subpasta, gerando novo nome. Retorna o novo caminho relativo ou null se a origem não existir.</summary>
    Task<string?> CopiarAsync(string? caminhoRelativoOrigem, string subpastaDestino);
}
