namespace Torneio.Domain.Entities;

public class WebhookEventoAsaas
{
    public Guid Id { get; private set; }
    public string EventoId { get; private set; } = null!;
    public string TipoEvento { get; private set; } = null!;
    public string? AsaasPaymentId { get; private set; }
    public string PayloadJson { get; private set; } = null!;
    public bool Processado { get; private set; }
    public string? ErroProcessamento { get; private set; }
    public DateTime RecebidoEm { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }

    private WebhookEventoAsaas() { }

    public static WebhookEventoAsaas Criar(
        string eventoId,
        string tipoEvento,
        string? asaasPaymentId,
        string payloadJson)
    {
        return new WebhookEventoAsaas
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId.Trim(),
            TipoEvento = tipoEvento.Trim(),
            AsaasPaymentId = string.IsNullOrWhiteSpace(asaasPaymentId) ? null : asaasPaymentId.Trim(),
            PayloadJson = payloadJson,
            Processado = false,
            RecebidoEm = DateTime.UtcNow
        };
    }

    public void MarcarProcessado()
    {
        Processado = true;
        ProcessadoEm = DateTime.UtcNow;
        ErroProcessamento = null;
    }

    public void MarcarErro(string erro)
    {
        Processado = false;
        ErroProcessamento = erro;
        ProcessadoEm = DateTime.UtcNow;
    }
}
