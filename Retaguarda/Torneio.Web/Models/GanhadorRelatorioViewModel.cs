namespace Torneio.Web.Models;

public class GanhadorRelatorioViewModel
{
    public int Posicao { get; init; }
    // Equipe
    public Guid? EquipeId { get; init; }
    public string? NomeEquipe { get; init; }
    public string? Capitao { get; init; }
    // Membro
    public Guid? MembroId { get; init; }
    public string? NomeMembro { get; init; }

    public decimal TotalPontos { get; init; }
    public DateTime PrimeiraCaptura { get; init; }
}

public class GanhadoresPageViewModel
{
    public List<GanhadorRelatorioViewModel> Equipes { get; init; } = [];
    public List<GanhadorRelatorioViewModel> Membros { get; init; } = [];
}
