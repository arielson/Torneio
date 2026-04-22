using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Financeiro;

public class CriarProdutoExtraMembroDto
{
    public Guid TorneioId { get; init; }
    public Guid ProdutoExtraTorneioId { get; init; }
    public Guid MembroId { get; init; }

    [Range(0.01, 999999999, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public decimal Quantidade { get; init; } = 1;

    [Range(0, 999999999, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
    public decimal ValorCobrado { get; init; }

    public string? Observacao { get; init; }
}
