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

    public SorteioController(TenantContext tenantContext, ISorteioAppServico servico)
        : base(tenantContext)
    {
        _servico = servico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var resultado = await _servico.ObterResultado();
        return View(resultado);
    }

    [HttpPost("realizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Realizar()
    {
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
        await _servico.LimparSorteio();
        TempData["Sucesso"] = "Sorteio limpo.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{sorteioEquipeId:guid}/ajustar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(Guid sorteioEquipeId, int novaPosicao)
    {
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
