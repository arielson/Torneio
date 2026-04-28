using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Auth;

public class AtualizarSenhaDto
{
    [Required(ErrorMessage = "A senha atual e obrigatoria.")]
    public string SenhaAtual { get; init; } = null!;

    [Required(ErrorMessage = "A nova senha e obrigatoria.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    public string NovaSenha { get; init; } = null!;
}
