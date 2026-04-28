using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Auth;

public class RedefinirSenhaDto
{
    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    public string NovaSenha { get; init; } = null!;
}
