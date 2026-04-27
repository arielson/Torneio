namespace Torneio.Application.DTOs.EspeciePeixe;

public class EspeciePeixeDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public string? NomeCientifico { get; init; }
    public string? FotoUrl { get; init; }
}
