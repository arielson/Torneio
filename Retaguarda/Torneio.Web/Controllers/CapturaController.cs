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
    private readonly IAnoTorneioServico _anoServico;

    public CapturaController(
        TenantContext tenantContext,
        ICapturaServico capturaServico,
        IAnoTorneioServico anoServico) : base(tenantContext)
    {
        _capturaServico = capturaServico;
        _anoServico = anoServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? anoTorneioId)
    {
        var anos = (await _anoServico.ListarPorTorneio(TenantContext.TorneioId)).ToList();
        ViewBag.Anos = anos;
        ViewBag.AnoTorneioIdSelecionado = anoTorneioId;

        if (anoTorneioId is null || anoTorneioId == Guid.Empty)
            return View(Enumerable.Empty<Torneio.Application.DTOs.Captura.CapturaDto>());

        var capturas = await _capturaServico.ListarPorAnoTorneio(anoTorneioId.Value);
        return View(capturas);
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id, Guid anoTorneioId)
    {
        await _capturaServico.Remover(id);
        TempData["Sucesso"] = "Captura removida.";
        return RedirectToAction(nameof(Index), new { anoTorneioId });
    }
}
