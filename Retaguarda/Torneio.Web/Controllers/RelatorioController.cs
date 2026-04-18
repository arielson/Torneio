using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/relatorios")]
public class RelatorioController : TorneioBaseController
{
    private readonly IRelatorioServico _relatorioServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ICapturaServico _capturaServico;

    public RelatorioController(
        TenantContext tenantContext,
        IRelatorioServico relatorioServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        ITorneioServico torneioServico,
        ICapturaServico capturaServico) : base(tenantContext)
    {
        _relatorioServico = relatorioServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _torneioServico = torneioServico;
        _capturaServico = capturaServico;
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

    [HttpGet("ganhadores")]
    public async Task<IActionResult> SelecionarGanhadores()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var equipes = (await _equipeServico.ListarTodos()).ToList();
        var capturas = (await _capturaServico.ListarTodos()).ToList();

        var ganhadores = equipes
            .Select(e => new GanhadorRelatorioViewModel
            {
                EquipeId = e.Id,
                NomeEquipe = e.Nome,
                Capitao = e.Capitao,
                TotalPontos = capturas
                    .Where(c => c.EquipeId == e.Id)
                    .Sum(c => c.Pontuacao)
            })
            .OrderByDescending(x => x.TotalPontos)
            .ThenBy(x => x.NomeEquipe)
            .Take(torneio.QtdGanhadores)
            .Select((x, index) => new GanhadorRelatorioViewModel
            {
                Posicao = index + 1,
                EquipeId = x.EquipeId,
                NomeEquipe = x.NomeEquipe,
                Capitao = x.Capitao,
                TotalPontos = x.TotalPontos
            })
            .ToList();

        ViewBag.Torneio = torneio;
        return View(ganhadores);
    }
}
