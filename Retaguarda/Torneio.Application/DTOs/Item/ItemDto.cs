namespace Torneio.Application.DTOs.Item;

public class ItemDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid EspeciePeixeId { get; init; }
    /// <summary>Nome da espécie — preenchido a partir de EspeciePeixe.Nome.</summary>
    public string Nome { get; init; } = null!;
    public string? NomeCientifico { get; init; }
    /// <summary>Foto da espécie — preenchida a partir de EspeciePeixe.FotoUrl.</summary>
    public string? FotoUrl { get; init; }
    public decimal? Comprimento { get; init; }
    public decimal FatorMultiplicador { get; init; }
}
