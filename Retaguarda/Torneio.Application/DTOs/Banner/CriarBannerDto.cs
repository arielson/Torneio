using System.ComponentModel.DataAnnotations;
namespace Torneio.Application.DTOs.Banner;
public class CriarBannerDto
{
    [Required] public Guid TorneioId { get; init; }
    [Required] public string ImagemUrl { get; set; } = null!;
    public int Ordem { get; init; } = 0;
}
