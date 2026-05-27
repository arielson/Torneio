namespace Torneio.Domain.Interfaces.Services;

public interface INotificacaoServico
{
    Task EnviarParaTokensAsync(IEnumerable<string> tokens, string titulo, string corpo);
}
