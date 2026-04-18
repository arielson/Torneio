using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Route("{slug}")]
public class PublicoController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;

    public PublicoController(TenantContext tenantContext, ITorneioServico torneioServico)
        : base(tenantContext)
    {
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null) return NotFound();
        return View(torneio);
    }
}
