using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/relatorios")]
public class RelatorioController : TorneioBaseController
{
    private readonly IRelatorioServico _relatorioServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;

    public RelatorioController(
        TenantContext tenantContext,
        IRelatorioServico relatorioServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        ITorneioServico torneioServico) : base(tenantContext)
    {
        _relatorioServico = relatorioServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        ViewBag.Torneio = torneio;
        return View();
    }

    [HttpGet("equipe")]
    public async Task<IActionResult> SelecionarEquipe()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var equipes = await _equipeServico.ListarTodos();
        ViewBag.Torneio = torneio;
        return View(equipes);
    }

    [HttpGet("equipe/download")]
    public async Task<IActionResult> DownloadEquipe(
        [FromQuery] Guid equipeId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _relatorioServico.GerarRelatorioEquipe(equipeId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"equipe_{equipeId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(SelecionarEquipe), new { slug = Slug });
        }
    }

    [HttpGet("membro")]
    public async Task<IActionResult> SelecionarMembro()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var membros = await _membroServico.ListarTodos();
        ViewBag.Torneio = torneio;
        return View(membros);
    }

    [HttpGet("membro/download")]
    public async Task<IActionResult> DownloadMembro(
        [FromQuery] Guid membroId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _relatorioServico.GerarRelatorioMembro(membroId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"membro_{membroId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(SelecionarMembro), new { slug = Slug });
        }
    }
}
