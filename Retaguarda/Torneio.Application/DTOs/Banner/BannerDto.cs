namespace Torneio.Application.DTOs.Banner;

public class BannerDto
{
    public Guid Id { get; init; }
    public string ImagemUrl { get; init; } = null!;
    public Guid TorneioId { get; init; }
    public string TorneioSlug { get; init; } = null!;
    public string TorneioNome { get; init; } = null!;
    public int Ordem { get; init; }
    public bool Ativo { get; init; }
    public string TipoDestino { get; init; } = "Torneio";
    public string? Destino { get; init; }
}
