namespace Torneio.Asaas;

public class AsaasOptions
{
    public const string Section = "Asaas";

    public string BaseUrlSandbox { get; set; } = "https://sandbox.asaas.com";
    public string BaseUrlProducao { get; set; } = "https://api.asaas.com";

    /// <summary>
    /// "Sandbox" ou "Producao"
    /// </summary>
    public string Ambiente { get; set; } = "Sandbox";

    /// <summary>
    /// Chave-mãe usada apenas para operações administrativas da conta (ex: configurar webhook global).
    /// A chave para criar cobranças vem de ConfiguracaoAsaasTorneio, não daqui.
    /// </summary>
    public string ChaveAdminConta { get; set; } = string.Empty;

    public string WebhookAuthToken { get; set; } = string.Empty;

    /// <summary>
    /// E-mail para receber notificações de falha na entrega do webhook.
    /// </summary>
    public string WebhookEmail { get; set; } = string.Empty;

    /// <summary>
    /// Prazo em dias corridos para crédito do cartão na conta Asaas após pagamento confirmado.
    /// </summary>
    public int PrazoCreditoCartaoDias { get; set; } = 32;

    public AsaasTaxasOptions Taxas { get; set; } = new();
    public AsaasWebhookOptions Webhook { get; set; } = new();

    public bool IsSandbox => Ambiente.Equals("Sandbox", StringComparison.OrdinalIgnoreCase);
}

public class AsaasTaxasOptions
{
    public decimal Pix { get; set; } = 1.99m;
    public decimal CartaoFixo { get; set; } = 0.49m;
    public decimal CartaoPercentual { get; set; } = 2.99m;
    public bool PromocaoAtiva { get; set; } = false;
    public decimal CartaoPercentualPromocional { get; set; } = 1.99m;
}

public class AsaasWebhookOptions
{
    public List<string> EventosAssinados { get; set; } = new();
}
