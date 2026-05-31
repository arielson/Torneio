using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Torneio.Application.Services.Interfaces;
using Torneio.Asaas;

namespace Torneio.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/webhook/asaas")]
public class AsaasWebhookController : ControllerBase
{
    private readonly IWebhookAsaasProcessador _processador;
    private readonly AsaasOptions _options;

    public AsaasWebhookController(
        IWebhookAsaasProcessador processador,
        IOptions<AsaasOptions> options)
    {
        _processador = processador;
        _options = options.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Receber()
    {
        var token = Request.Headers["asaas-access-token"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token) || token != _options.WebhookAuthToken)
            return Unauthorized();

        string payloadJson;
        using (var reader = new StreamReader(Request.Body))
            payloadJson = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(payloadJson))
            return BadRequest();

        await _processador.ProcessarAsync(payloadJson);

        return Ok();
    }
}
