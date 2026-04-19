namespace Torneio.Application.DTOs.Sorteio;

public class SorteioPreCondicoesDto
{
    public int QtdEquipes { get; init; }
    public int TotalVagas { get; init; }
    public int QtdMembros { get; init; }
    public bool Valido { get; init; }
    public string? MensagemErro { get; init; }
}
