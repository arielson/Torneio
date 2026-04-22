namespace Torneio.Application.DTOs.Financeiro;

public class ResumoFinanceiroPorCategoriaDto
{
    public string Chave { get; init; } = null!;
    public string Label { get; init; } = null!;
    public int Quantidade { get; init; }
    public decimal Total { get; init; }
}
