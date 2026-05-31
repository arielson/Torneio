namespace Torneio.Application.DTOs.Asaas;

public class CobrancaAsaasDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid MembroId { get; init; }
    public Guid ParcelaTorneioId { get; init; }
    public string AsaasPaymentId { get; init; } = null!;
    public string? AsaasInvoiceUrl { get; init; }
    public string Status { get; init; } = null!;
    public string? FormaPagamento { get; init; }
    public decimal ValorOriginal { get; init; }
    public decimal? TaxaAsaas { get; init; }
    public DateTime Vencimento { get; init; }
    public DateTime? DataPrevisaoCredito { get; init; }
    public DateTime? DataCreditoEfetivo { get; init; }
    public DateTime CriadoEm { get; init; }
}
