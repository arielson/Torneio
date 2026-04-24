namespace Torneio.Application.DTOs.Financeiro;

public class TorneioFinanceiroConfigDto
{
    public Guid TorneioId { get; init; }
    public decimal ValorPorMembro { get; init; }
    public int QuantidadeParcelas { get; init; }
    public DateTime? DataPrimeiroVencimento { get; init; }
    public decimal TaxaInscricaoValor { get; init; }
    public DateTime? DataVencimentoTaxaInscricao { get; init; }
    public bool PossuiConfiguracaoAnterior { get; init; }
    public List<ValorParcelaDto> ValoresParcelas { get; init; } = [];
}
