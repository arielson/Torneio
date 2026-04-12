using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/capturas — Fiscal e AdminTorneio
/// </summary>
[Authorize]
[Route("api/{slug}/capturas")]
public class CapturaController : BaseController
{
    private readonly ICapturaServico _servico;

    public CapturaController(ICapturaServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid anoTorneioId,
        [FromQuery] Guid? equipeId,
        [FromQuery] Guid? membroId)
    {
        if (equipeId.HasValue)
            return Ok(await _servico.ListarPorEquipe(equipeId.Value, anoTorneioId));

        if (membroId.HasValue)
            return Ok(await _servico.ListarPorMembro(membroId.Value, anoTorneioId));

        return Ok(await _servico.ListarPorAnoTorneio(anoTorneioId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCapturaDto dto)
    {
        var criado = await _servico.Registrar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }
}
