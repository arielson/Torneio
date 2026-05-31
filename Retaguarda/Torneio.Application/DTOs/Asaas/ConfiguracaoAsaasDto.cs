namespace Torneio.Application.DTOs.Asaas;

public class ConfiguracaoAsaasDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string? ChaveApiAsaas { get; init; }
    public string StatusChave { get; init; } = null!;
    public string? AsaasAccountId { get; init; }
    public bool AceitarPix { get; init; }
    public bool AceitarCartaoCredito { get; init; }
    public DateTime? DataAtivacao { get; init; }
}
