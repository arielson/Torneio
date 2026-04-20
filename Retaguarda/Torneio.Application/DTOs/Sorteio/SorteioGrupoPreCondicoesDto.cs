namespace Torneio.Application.DTOs.Sorteio;

public class SorteioGrupoPreCondicoesDto
{
    public int QtdGrupos { get; init; }
    public int QtdEquipes { get; init; }
    public bool Valido { get; init; }
    public string? MensagemErro { get; init; }
}
