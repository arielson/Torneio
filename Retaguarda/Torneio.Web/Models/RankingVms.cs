namespace Torneio.Web.Models;

public class RankingEquipeVm
{
    public int Posicao { get; set; }
    public Guid EquipeId { get; set; }
    public string NomeEquipe { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public decimal TotalPontos { get; set; }
    public int QtdCapturas { get; set; }
    public DateTime PrimeiraCaptura { get; set; }
}

public class RankingMembroVm
{
    public int Posicao { get; set; }
    public Guid MembroId { get; set; }
    public string NomeMembro { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public string NomeEquipe { get; set; } = null!;
    public decimal TotalPontos { get; set; }
    public DateTime PrimeiraCaptura { get; set; }
    public List<RankingCapturaVm> Capturas { get; set; } = new();
}

public class RankingCapturaVm
{
    public string NomeItem { get; set; } = null!;
    public decimal TamanhoMedida { get; set; }
    public decimal FatorMultiplicador { get; set; }
    public decimal Pontuacao { get; set; }
    public string? FotoUrl { get; set; }
    public DateTime DataHora { get; set; }
}
