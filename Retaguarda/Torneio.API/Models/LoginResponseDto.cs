namespace Torneio.API.Models;

public class LoginResponseDto
{
    public string Token { get; init; } = null!;
    public string Perfil { get; init; } = null!;
    public string? Slug { get; init; }
    public string Nome { get; init; } = null!;
    public DateTime ExpiraEm { get; init; }
}
