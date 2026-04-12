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
    private readonly IAnoTorneioServico _anoServico;

    public SorteioController(TenantContext tenantContext, ISorteioAppServico servico, IAnoTorneioServico anoServico)
        : base(tenantContext)
    {
        _servico = servico;
        _anoServico = anoServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var anos = await _anoServico.ListarPorTorneio(TenantContext.TorneioId);
        return View(anos);
    }

    [HttpGet("{anoId:guid}")]
    public async Task<IActionResult> Detalhe(Guid anoId)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;

        var resultado = await _servico.ObterResultado(anoId);
        return View(resultado);
    }

    [HttpPost("{anoId:guid}/realizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Realizar(Guid anoId)
    {
        try
        {
            await _servico.RealizarSorteio(anoId);
            TempData["Sucesso"] = "Sorteio realizado com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Detalhe), new { slug = Slug, anoId });
    }

    [HttpPost("{anoId:guid}/limpar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Limpar(Guid anoId)
    {
        await _servico.LimparSorteio(anoId);
        TempData["Sucesso"] = "Sorteio limpo.";
        return RedirectToAction(nameof(Detalhe), new { slug = Slug, anoId });
    }

    [HttpPost("{anoId:guid}/{sorteioEquipeId:guid}/ajustar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(Guid anoId, Guid sorteioEquipeId, int novaPosicao)
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
        return RedirectToAction(nameof(Detalhe), new { slug = Slug, anoId });
    }
}
