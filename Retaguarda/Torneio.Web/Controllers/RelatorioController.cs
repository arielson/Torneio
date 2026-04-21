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

        var capturas = (await _capturaServico.ListarTodos()).ToList();
        var vm = new GanhadoresPageViewModel();

        if (torneio.PremiacaoPorEquipe)
        {
            var equipes = (await _equipeServico.ListarTodos()).ToList();
            vm.Equipes.AddRange(equipes
                .Select(e => new GanhadorRelatorioViewModel
                {
                    EquipeId = e.Id,
                    NomeEquipe = e.Nome,
                    Capitao = e.Capitao,
                    TotalPontos = capturas.Where(c => c.EquipeId == e.Id).Sum(c => c.Pontuacao)
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.NomeEquipe)
                .Take(torneio.QtdGanhadores)
                .Select((x, i) => new GanhadorRelatorioViewModel
                {
                    Posicao = i + 1, EquipeId = x.EquipeId,
                    NomeEquipe = x.NomeEquipe, Capitao = x.Capitao,
                    TotalPontos = x.TotalPontos
                }));
        }

        if (torneio.PremiacaoPorMembro)
        {
            var membros = (await _membroServico.ListarTodos()).ToList();
            vm.Membros.AddRange(membros
                .Select(m => new GanhadorRelatorioViewModel
                {
                    MembroId = m.Id,
                    NomeMembro = m.Nome,
                    TotalPontos = capturas.Where(c => c.MembroId == m.Id).Sum(c => c.Pontuacao)
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.NomeMembro)
                .Take(torneio.QtdGanhadores)
                .Select((x, i) => new GanhadorRelatorioViewModel
                {
                    Posicao = i + 1, MembroId = x.MembroId,
                    NomeMembro = x.NomeMembro, TotalPontos = x.TotalPontos
                }));
        }

        ViewBag.Torneio = torneio;
        return View(vm);
    }

    [HttpGet("maiores-capturas")]
    public async Task<IActionResult> MaioresCapturas()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        ViewBag.Torneio = torneio;
        return View(new MaioresCapturasFiltroViewModel());
    }

    [HttpGet("maiores-capturas/download")]
    public async Task<IActionResult> DownloadMaioresCapturas([FromQuery] int quantidade = 1)
    {
        try
        {
            var bytes = await _relatorioServico.GerarRelatorioMaioresCapturas(quantidade);
            return File(bytes, "application/pdf", $"maiores_capturas_{quantidade}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(MaioresCapturas), new { slug = Slug });
        }
    }
}
