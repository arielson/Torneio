using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Asaas;

public class GerarCobrancaDto
{
    public Guid TorneioId { get; set; }
    public Guid ParcelaTorneioId { get; set; }
    public FormaPagamentoAsaas FormaPagamento { get; set; }
    public string? CpfOverride { get; set; }
}
