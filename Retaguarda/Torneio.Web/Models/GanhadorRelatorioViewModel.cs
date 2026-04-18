namespace Torneio.Web.Models;

public class GanhadorRelatorioViewModel
{
    public int Posicao { get; init; }
    public Guid EquipeId { get; init; }
    public string NomeEquipe { get; init; } = null!;
    public string Capitao { get; init; } = null!;
    public decimal TotalPontos { get; init; }
}
