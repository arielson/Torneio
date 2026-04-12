using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// POST /api/{slug}/sync — Fiscal envia capturas offline acumuladas
/// </summary>
[Authorize]
[Route("api/{slug}/sync")]
public class SyncController : BaseController
{
    private readonly ICapturaServico _capturaServico;

    public SyncController(ICapturaServico capturaServico) => _capturaServico = capturaServico;

    [HttpPost]
    public async Task<IActionResult> Sincronizar([FromBody] IEnumerable<RegistrarCapturaDto> capturas)
    {
        var total = await _capturaServico.SincronizarLote(capturas);
        return Ok(new { sincronizadas = total });
    }
}
