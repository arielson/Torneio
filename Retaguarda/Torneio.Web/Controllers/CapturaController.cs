using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/capturas")]
public class CapturaController : TorneioBaseController
{
    private readonly ICapturaServico _capturaServico;

    public CapturaController(TenantContext tenantContext, ICapturaServico capturaServico)
        : base(tenantContext)
    {
        _capturaServico = capturaServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var capturas = await _capturaServico.ListarTodos();
        return View(capturas);
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _capturaServico.Remover(id);
        TempData["Sucesso"] = "Captura removida.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
