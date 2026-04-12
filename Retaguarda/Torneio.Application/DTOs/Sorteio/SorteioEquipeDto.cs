namespace Torneio.Application.DTOs.Sorteio;

public class SorteioEquipeDto
{
    public Guid Id { get; init; }
    public Guid EquipeId { get; init; }
    public string NomeEquipe { get; init; } = null!;
    public Guid MembroId { get; init; }
    public string NomeMembro { get; init; } = null!;
    public int Posicao { get; init; }
}
