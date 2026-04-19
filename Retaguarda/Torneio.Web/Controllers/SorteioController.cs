using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/sorteio")]
public class SorteioController : TorneioBaseController
{
    private readonly ISorteioAppServico _servico;
    private readonly ITorneioServico _torneioServico;

    public SorteioController(
        TenantContext tenantContext,
        ISorteioAppServico servico,
        ITorneioServico torneioServico)
        : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, "Nenhum", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "TorneioAdmin", new { slug = Slug });

        ViewBag.Torneio = torneio;
        ViewBag.PreCondicoes = await _servico.VerificarPreCondicoes();
        var resultado = await _servico.ObterResultado();
        return View(resultado);
    }

    [HttpPost("realizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Realizar()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, "Nenhum", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "TorneioAdmin", new { slug = Slug });

        try
        {
            await _servico.RealizarSorteio();
            TempData["Sucesso"] = "Sorteio realizado com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("limpar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Limpar()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, "Nenhum", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "TorneioAdmin", new { slug = Slug });

        await _servico.LimparSorteio();
        TempData["Sucesso"] = "Sorteio limpo.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{sorteioEquipeId:guid}/ajustar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(Guid sorteioEquipeId, int novaPosicao)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, "Nenhum", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "TorneioAdmin", new { slug = Slug });

        try
        {
            await _servico.AjustarPosicao(sorteioEquipeId, novaPosicao);
            TempData["Sucesso"] = "Posição ajustada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
