using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// GET /api/{slug}/config → público, sem auth
/// </summary>
[AllowAnonymous]
[Route("api/{slug}/config")]
public class ConfigController : BaseController
{
    private readonly ITorneioServico _torneioServico;

    public ConfigController(ITorneioServico torneioServico)
    {
        _torneioServico = torneioServico;
    }

    [HttpGet]
    public async Task<IActionResult> GetConfig([FromRoute] string slug)
    {
        var config = await _torneioServico.ObterPorSlug(slug);
        if (config is null) return NotFound(new { erro = $"Torneio '{slug}' não encontrado." });
        return Ok(config);
    }
}
