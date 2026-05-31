namespace Torneio.Application.Services.Interfaces;

public interface ISmsService
{
    Task EnviarCodigoAsync(string numero, string codigo);
    Task EnviarMensagemAsync(string numero, string mensagem);
}
