using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Route("{slug}")]
public class PublicoController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IPatrocinadorServico _patrocinadorServico;

    public PublicoController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IPatrocinadorServico patrocinadorServico)
        : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _patrocinadorServico = patrocinadorServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null) return NotFound();
        ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(torneio.Id);
        return View(torneio);
    }
}
