using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/admin")]
public class TorneioAdminController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;

    public TorneioAdminController(TenantContext tenantContext, ITorneioServico torneioServico)
        : base(tenantContext)
    {
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        return View(torneio);
    }

    [HttpPost("liberar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar()
    {
        try
        {
            await _torneioServico.Liberar(TenantContext.TorneioId);
            TempData["Sucesso"] = "Torneio liberado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("finalizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar()
    {
        try
        {
            await _torneioServico.Finalizar(TenantContext.TorneioId);
            TempData["Sucesso"] = "Torneio finalizado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("reabrir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reabrir()
    {
        try
        {
            await _torneioServico.Reabrir(TenantContext.TorneioId);
            TempData["Sucesso"] = "Torneio reaberto.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("clonar")]
    public IActionResult Clonar() => View();

    [HttpPost("clonar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clonar(string novoSlug, string novoNome)
    {
        try
        {
            var novo = await _torneioServico.ClonarTorneio(TenantContext.TorneioId, novoSlug, novoNome);
            TempData["Sucesso"] = $"Torneio \"{novo.NomeTorneio}\" criado a partir desta edição.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            return View();
        }
    }
}
