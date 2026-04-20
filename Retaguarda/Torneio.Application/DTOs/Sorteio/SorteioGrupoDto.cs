namespace Torneio.Application.DTOs.Sorteio;

public class SorteioGrupoDto
{
    public Guid Id { get; init; }
    public Guid GrupoId { get; init; }
    public string NomeGrupo { get; init; } = null!;
    public Guid EquipeId { get; init; }
    public string NomeEquipe { get; init; } = null!;
    public int Posicao { get; init; }
    public List<string> NomesMembros { get; init; } = new();
}
