using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Route("{slug}")]
public class PublicoController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IAnoTorneioServico _anoServico;

    public PublicoController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IAnoTorneioServico anoServico) : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _anoServico = anoServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null) return NotFound();

        var anos = await _anoServico.ListarPorTorneio(torneio.Id);
        ViewBag.Torneio = torneio;
        return View(anos);
    }
}
