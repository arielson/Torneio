using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Route("api/{slug}/seguidor")]
public class SeguidorController : BaseController
{
    private readonly ISeguidorServico _servico;
    private readonly TenantContext _tenantContext;

    public SeguidorController(ISeguidorServico servico, TenantContext tenantContext)
    {
        _servico = servico;
        _tenantContext = tenantContext;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarSeguidorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceToken))
            return BadRequest("deviceToken é obrigatório.");

        await _servico.RegistrarAsync(
            _tenantContext.TorneioId,
            request.DeviceToken,
            request.Plataforma ?? "Android");

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Remover([FromQuery] string deviceToken)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            return BadRequest("deviceToken é obrigatório.");

        await _servico.RemoverAsync(_tenantContext.TorneioId, deviceToken);
        return Ok();
    }
}

public record RegistrarSeguidorRequest(string DeviceToken, string? Plataforma);
