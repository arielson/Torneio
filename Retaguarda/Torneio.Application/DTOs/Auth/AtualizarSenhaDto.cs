using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Auth;

public class AtualizarSenhaDto
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string SenhaAtual { get; init; } = null!;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    public string NovaSenha { get; init; } = null!;
}
