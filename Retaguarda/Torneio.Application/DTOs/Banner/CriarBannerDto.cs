using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Banner;

public class CriarBannerDto
{
    [Required] public Guid TorneioId { get; init; }
    [Required] public string ImagemUrl { get; set; } = null!;
    public int Ordem { get; init; } = 0;
    public TipoDestinoBanner TipoDestino { get; init; } = TipoDestinoBanner.Torneio;

    /// <summary>Depende de TipoDestino — ver Banner.Destino para semântica.</summary>
    [MaxLength(500)]
    public string? Destino { get; init; }
}
