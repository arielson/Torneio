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
    public decimal? MaiorCaptura { get; init; }
    public string? NomeItemMaiorCaptura { get; init; }
    public DateTime PrimeiraCaptura { get; init; }
}

public class GanhadoresPageViewModel
{
    public int QuantidadeEquipes { get; set; }
    public int QuantidadeMembrosPontuacao { get; set; }
    public int QuantidadeMembrosMaiorCaptura { get; set; }
    public bool ExibirMaiorCaptura { get; set; }
    public bool FiltrosInformados { get; set; }
    public List<GanhadorRelatorioViewModel> Equipes { get; init; } = [];
    public List<GanhadorRelatorioViewModel> MembrosPontuacao { get; init; } = [];
    public List<GanhadorRelatorioViewModel> MembrosMaiorCaptura { get; init; } = [];
}
