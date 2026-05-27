using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Torneio.Domain.Interfaces.Services;
using FcmConfig = Torneio.Infrastructure.Services.Options.FcmOptions;

namespace Torneio.Infrastructure.Services;

public class FcmNotificacaoServico : INotificacaoServico
{
    private readonly ILogger<FcmNotificacaoServico> _logger;
    private readonly bool _configurado;

    public FcmNotificacaoServico(IOptions<FcmConfig> options, ILogger<FcmNotificacaoServico> logger)
    {
        _logger = logger;
        var path = options.Value.ServiceAccountPath;

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _logger.LogWarning("FCM não configurado — notificações push desativadas. Configure Fcm:ServiceAccountPath no appsettings.");
            return;
        }

        if (FirebaseApp.DefaultInstance is null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path)
            });
        }

        _configurado = true;
    }

    public async Task EnviarParaTokensAsync(IEnumerable<string> tokens, string titulo, string corpo)
    {
        if (!_configurado) return;

        var tokenList = tokens.ToList();
        if (tokenList.Count == 0) return;

        // FCM aceita até 500 tokens por lote
        foreach (var lote in tokenList.Chunk(500))
        {
            var mensagem = new MulticastMessage
            {
                Tokens = lote.ToList(),
                Notification = new Notification { Title = titulo, Body = corpo },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification { Sound = "default" }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default" }
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(mensagem);
                _logger.LogInformation("FCM: {Sucesso} enviados, {Falha} falhas.", response.SuccessCount, response.FailureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação FCM.");
            }
        }
    }
}
