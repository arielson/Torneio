using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "O usuário é obrigatório.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; init; } = null!;

    public string? Slug { get; init; }
}
