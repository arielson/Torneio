using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class CobrancaAsaas
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid MembroId { get; private set; }
    public Guid ParcelaTorneioId { get; private set; }
    public string AsaasPaymentId { get; private set; } = null!;
    public string? AsaasCustomerId { get; private set; }
    public string? AsaasInvoiceUrl { get; private set; }
    public StatusCobrancaAsaas Status { get; private set; }
    public FormaPagamentoAsaas? FormaPagamento { get; private set; }
    public decimal ValorOriginal { get; private set; }
    public decimal? TaxaAsaas { get; private set; }
    public DateTime Vencimento { get; private set; }
    public DateTime? DataPrevisaoCredito { get; private set; }
    public DateTime? DataCreditoEfetivo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    private CobrancaAsaas() { }

    public static CobrancaAsaas Criar(
        Guid torneioId,
        Guid membroId,
        Guid parcelaTorneioId,
        string asaasPaymentId,
        string? asaasCustomerId,
        string? asaasInvoiceUrl,
        decimal valorOriginal,
        DateTime vencimento)
    {
        var now = DateTime.UtcNow;
        return new CobrancaAsaas
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            MembroId = membroId,
            ParcelaTorneioId = parcelaTorneioId,
            AsaasPaymentId = asaasPaymentId.Trim(),
            AsaasCustomerId = string.IsNullOrWhiteSpace(asaasCustomerId) ? null : asaasCustomerId.Trim(),
            AsaasInvoiceUrl = string.IsNullOrWhiteSpace(asaasInvoiceUrl) ? null : asaasInvoiceUrl.Trim(),
            Status = StatusCobrancaAsaas.Pendente,
            ValorOriginal = valorOriginal,
            Vencimento = vencimento,
            CriadoEm = now,
            AtualizadoEm = now
        };
    }

    public void AtualizarStatus(
        StatusCobrancaAsaas novoStatus,
        FormaPagamentoAsaas? formaPagamento = null,
        decimal? taxaAsaas = null,
        DateTime? dataPrevisaoCredito = null,
        DateTime? dataCreditoEfetivo = null)
    {
        Status = novoStatus;
        AtualizadoEm = DateTime.UtcNow;

        if (formaPagamento.HasValue)
            FormaPagamento = formaPagamento;

        if (taxaAsaas.HasValue)
            TaxaAsaas = taxaAsaas;

        if (dataPrevisaoCredito.HasValue)
            DataPrevisaoCredito = dataPrevisaoCredito;

        if (dataCreditoEfetivo.HasValue)
            DataCreditoEfetivo = dataCreditoEfetivo;
    }
}
