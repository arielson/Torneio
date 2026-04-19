namespace Torneio.Application.DTOs.Sorteio;

public class ConfirmarSorteioItemDto
{
    public Guid EquipeId { get; set; }
    public string NomeEquipe { get; set; } = string.Empty;
    public Guid MembroId { get; set; }
    public string NomeMembro { get; set; } = string.Empty;
    public int Posicao { get; set; }
}
