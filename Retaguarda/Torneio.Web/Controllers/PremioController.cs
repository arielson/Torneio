using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Premio;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/premios")]
public class PremioController : TorneioBaseController
{
    private readonly IPremioServico _premioServico;

    public PremioController(TenantContext tenantContext, IPremioServico premioServico)
        : base(tenantContext)
    {
        _premioServico = premioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var premios = await _premioServico.ListarPorTorneio(TenantContext.TorneioId);
        return View(premios);
    }

    [HttpPost("adicionar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(CriarPremioDto dto)
    {
        try
        {
            await _premioServico.Criar(TenantContext.TorneioId, new CriarPremioDto
            {
                Posicao = dto.Posicao,
                Descricao = dto.Descricao,
            });
            TempData["Sucesso"] = "Prêmio adicionado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _premioServico.Remover(id);
        TempData["Sucesso"] = "Prêmio removido.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
