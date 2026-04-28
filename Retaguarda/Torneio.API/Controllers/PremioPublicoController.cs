using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

[AllowAnonymous]
[Route("api/{slug}/premios")]
public class PremioPublicoController : BaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IPremioServico _premioServico;

    public PremioPublicoController(ITorneioServico torneioServico, IPremioServico premioServico)
    {
        _torneioServico = torneioServico;
        _premioServico = premioServico;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null)
            return NotFound(new { erro = $"Torneio '{slug}' não encontrado." });

        if (torneio.Status != "Liberado")
            return Ok(Array.Empty<object>());

        var premios = (await _premioServico.ListarPorTorneio(torneio.Id))
            .OrderBy(p => p.Posicao)
            .ToList();

        return Ok(premios);
    }
}
