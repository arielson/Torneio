using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Financeiro;

public class AtualizarTorneioFinanceiroDto
{
    [Range(0, 999999999, ErrorMessage = "O valor por pescador nao pode ser negativo.")]
    public decimal ValorPorMembro { get; init; }

    [Range(0, 999, ErrorMessage = "A quantidade de parcelas deve estar entre 0 e 999.")]
    public int QuantidadeParcelas { get; init; }

    public DateTime? DataPrimeiroVencimento { get; init; }
    [Range(0, 999999999, ErrorMessage = "A taxa de inscricao nao pode ser negativa.")]
    public decimal TaxaInscricaoValor { get; init; }
    public DateTime? DataVencimentoTaxaInscricao { get; init; }
    public bool ConfirmarSubstituicao { get; init; }
    public List<ValorParcelaDto> ValoresParcelas { get; init; } = [];
}
