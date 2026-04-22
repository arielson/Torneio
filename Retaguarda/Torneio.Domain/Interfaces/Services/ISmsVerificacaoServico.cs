namespace Torneio.Domain.Interfaces.Services;

public interface ISmsVerificacaoServico
{
    Task EnviarCodigo(string celularE164);
    Task<bool> ValidarCodigo(string celularE164, string codigo);
}
