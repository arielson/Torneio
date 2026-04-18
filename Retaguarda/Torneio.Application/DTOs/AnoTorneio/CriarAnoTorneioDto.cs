using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.AnoTorneio;

public class CriarAnoTorneioDto
{
    public Guid TorneioId { get; init; }

    [Required(ErrorMessage = "O título da edição é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O título deve ter entre 2 e 200 caracteres.")]
    public string Titulo { get; init; } = null!;
}
