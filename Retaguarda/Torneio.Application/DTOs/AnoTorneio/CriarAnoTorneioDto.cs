using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.AnoTorneio;

public class CriarAnoTorneioDto
{
    public Guid TorneioId { get; init; }

    [Range(2000, 2100, ErrorMessage = "Informe um ano válido.")]
    public int Ano { get; init; }
}
