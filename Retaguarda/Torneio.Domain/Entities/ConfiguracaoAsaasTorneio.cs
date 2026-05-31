using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class ConfiguracaoAsaasTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string? ChaveApiAsaas { get; private set; }
    public StatusChaveAsaas StatusChave { get; private set; }
    public string? AsaasAccountId { get; private set; }
    public bool AceitarPix { get; private set; }
    public bool AceitarCartaoCredito { get; private set; }
    public DateTime? DataAtivacao { get; private set; }

    private ConfiguracaoAsaasTorneio() { }

    public static ConfiguracaoAsaasTorneio Criar(Guid torneioId)
    {
        return new ConfiguracaoAsaasTorneio
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            StatusChave = StatusChaveAsaas.NaoConfigurada,
            AceitarPix = true,
            AceitarCartaoCredito = false
        };
    }

    public void ConfigurarChave(string chaveApiAsaas, string? asaasAccountId = null)
    {
        ChaveApiAsaas = chaveApiAsaas.Trim();
        AsaasAccountId = string.IsNullOrWhiteSpace(asaasAccountId) ? null : asaasAccountId.Trim();
        StatusChave = StatusChaveAsaas.Ativa;
        DataAtivacao ??= DateTime.UtcNow;
    }

    public void AtualizarFormasPagamento(bool aceitarPix, bool aceitarCartaoCredito)
    {
        AceitarPix = aceitarPix;
        AceitarCartaoCredito = aceitarCartaoCredito;
    }

    public void Desativar() => StatusChave = StatusChaveAsaas.Inativa;

    public void Reativar() => StatusChave = StatusChaveAsaas.Ativa;
}
