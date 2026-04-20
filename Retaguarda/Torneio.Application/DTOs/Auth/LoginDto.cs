using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "O usuario e obrigatorio.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "A senha e obrigatoria.")]
    public string Senha { get; init; } = null!;

    public string? Slug { get; init; }
    public string? Perfil { get; init; }
}
