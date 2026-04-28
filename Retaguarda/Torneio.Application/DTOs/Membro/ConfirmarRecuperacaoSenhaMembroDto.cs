using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Membro;

public class ConfirmarRecuperacaoSenhaMembroDto
{
    [Required(ErrorMessage = "O usuario e obrigatorio.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "O celular e obrigatorio.")]
    public string Celular { get; init; } = null!;

    [Required(ErrorMessage = "O codigo e obrigatorio.")]
    public string Codigo { get; init; } = null!;

    [Required(ErrorMessage = "A nova senha e obrigatoria.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    public string NovaSenha { get; init; } = null!;
}
