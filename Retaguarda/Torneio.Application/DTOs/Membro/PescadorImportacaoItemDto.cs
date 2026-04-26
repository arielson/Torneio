namespace Torneio.Application.DTOs.Membro;

public class PescadorImportacaoItemDto
{
    public string PescaProId { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string? Celular { get; set; }
    public string? FotoUrl { get; set; }
}
