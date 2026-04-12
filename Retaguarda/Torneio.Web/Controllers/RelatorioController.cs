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
    private readonly IAnoTorneioServico _anoServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;

    public RelatorioController(
        TenantContext tenantContext,
        IRelatorioServico relatorioServico,
        IAnoTorneioServico anoServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        ITorneioServico torneioServico) : base(tenantContext)
    {
        _relatorioServico = relatorioServico;
        _anoServico = anoServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var anos = await _anoServico.ListarPorTorneio(TenantContext.TorneioId);
        ViewBag.Torneio = torneio;
        return View(anos);
    }

    [HttpGet("equipe")]
    public async Task<IActionResult> SelecionarEquipe([FromQuery] Guid anoTorneioId)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var ano = await _anoServico.ObterPorId(anoTorneioId);
        if (ano is null) return NotFound();

        var equipes = await _equipeServico.ListarPorAnoTorneio(anoTorneioId);
        ViewBag.Torneio = torneio;
        ViewBag.Ano = ano;
        return View(equipes);
    }

    [HttpGet("equipe/download")]
    public async Task<IActionResult> DownloadEquipe(
        [FromQuery] Guid anoTorneioId,
        [FromQuery] Guid equipeId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _relatorioServico.GerarRelatorioEquipe(anoTorneioId, equipeId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"equipe_{equipeId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(SelecionarEquipe), new { slug = Slug, anoTorneioId });
        }
    }

    [HttpGet("membro")]
    public async Task<IActionResult> SelecionarMembro([FromQuery] Guid anoTorneioId)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var ano = await _anoServico.ObterPorId(anoTorneioId);
        if (ano is null) return NotFound();

        var membros = await _membroServico.ListarPorAnoTorneio(anoTorneioId);
        ViewBag.Torneio = torneio;
        ViewBag.Ano = ano;
        return View(membros);
    }

    [HttpGet("membro/download")]
    public async Task<IActionResult> DownloadMembro(
        [FromQuery] Guid anoTorneioId,
        [FromQuery] Guid membroId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _relatorioServico.GerarRelatorioMembro(anoTorneioId, membroId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"membro_{membroId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(SelecionarMembro), new { slug = Slug, anoTorneioId });
        }
    }
}
