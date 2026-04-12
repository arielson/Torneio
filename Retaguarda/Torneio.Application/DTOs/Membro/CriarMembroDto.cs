using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Membro;

public class CriarMembroDto
{
    public Guid TorneioId { get; init; }
    public Guid AnoTorneioId { get; init; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    public string? FotoUrl { get; init; }
}
