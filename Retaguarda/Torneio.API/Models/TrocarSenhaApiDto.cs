namespace Torneio.API.Models;

public class TrocarSenhaApiDto
{
    public string Usuario { get; init; } = null!;
    public string SenhaAtual { get; init; } = null!;
    public string NovaSenha { get; init; } = null!;
}
