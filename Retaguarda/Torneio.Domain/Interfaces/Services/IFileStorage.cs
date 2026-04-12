namespace Torneio.Domain.Interfaces.Services;

public interface IFileStorage
{
    Task<string> SalvarAsync(Stream conteudo, string nomeArquivo, string subpasta);
    Task RemoverAsync(string caminhoRelativo);
    string ObterUrlPublica(string caminhoRelativo);
}
