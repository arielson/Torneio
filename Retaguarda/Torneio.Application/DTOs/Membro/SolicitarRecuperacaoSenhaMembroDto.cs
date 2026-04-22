using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Membro;

public class SolicitarRecuperacaoSenhaMembroDto
{
    [Required(ErrorMessage = "O usuario e obrigatorio.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "O celular e obrigatorio.")]
    public string Celular { get; init; } = null!;
}
