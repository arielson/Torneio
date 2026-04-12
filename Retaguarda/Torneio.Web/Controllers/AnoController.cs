using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.AnoTorneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/anos")]
public class AnoController : TorneioBaseController
{
    private readonly IAnoTorneioServico _servico;

    public AnoController(TenantContext tenantContext, IAnoTorneioServico servico) : base(tenantContext)
    {
        _servico = servico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var anos = await _servico.ListarPorTorneio(TenantContext.TorneioId);
        return View(anos);
    }

    [HttpGet("criar")]
    public IActionResult Criar() => View(new CriarAnoTorneioDto { TorneioId = TenantContext.TorneioId });

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarAnoTorneioDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _servico.Criar(new CriarAnoTorneioDto { TorneioId = TenantContext.TorneioId, Ano = dto.Ano });
            TempData["Sucesso"] = "Ano criado com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/liberar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar(Guid id)
    {
        await _servico.Liberar(id);
        TempData["Sucesso"] = "Ano liberado.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/finalizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        await _servico.Finalizar(id);
        TempData["Sucesso"] = "Ano finalizado.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/reabrir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reabrir(Guid id)
    {
        await _servico.Reabrir(id);
        TempData["Sucesso"] = "Ano reaberto.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("{id:guid}/replicar")]
    public async Task<IActionResult> Replicar(Guid id)
    {
        var ano = await _servico.ObterPorId(id);
        if (ano is null) return NotFound();
        ViewBag.AnoOrigem = ano;
        ViewBag.NovoAno = ano.Ano + 1;
        return View();
    }

    [HttpPost("{id:guid}/replicar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Replicar(Guid id, int novoAno)
    {
        try
        {
            await _servico.ReplicarAno(id, novoAno);
            TempData["Sucesso"] = $"Ano {novoAno} criado por replicação.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
    }
}
