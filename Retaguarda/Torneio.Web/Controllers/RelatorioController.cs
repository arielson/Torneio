using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
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
    public async Task<IActionResult> SelecionarGanhadores(
        [FromQuery] int? quantidadeEquipes,
        [FromQuery] int? quantidadeMembrosPontuacao,
        [FromQuery] int? quantidadeMembrosMaiorCaptura)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var vm = new GanhadoresPageViewModel
        {
            QuantidadeEquipes = quantidadeEquipes ?? 3,
            QuantidadeMembrosPontuacao = quantidadeMembrosPontuacao ?? 3,
            QuantidadeMembrosMaiorCaptura = quantidadeMembrosMaiorCaptura ?? 3,
            ExibirMaiorCaptura = string.Equals(torneio.TipoTorneio, nameof(TipoTorneio.Pesca), StringComparison.OrdinalIgnoreCase),
            FiltrosInformados = quantidadeEquipes.HasValue || quantidadeMembrosPontuacao.HasValue || quantidadeMembrosMaiorCaptura.HasValue
        };

        if (!vm.FiltrosInformados)
        {
            ViewBag.Torneio = torneio;
            return View(vm);
        }

        var todasCapturas = (await _capturaServico.ListarTodos())
            .Where(c => !c.Invalidada)
            .ToList();

        var capturasPontuacao = torneio.ApenasMaiorCapturaPorPescador
            ? todasCapturas
                .GroupBy(c => c.MembroId)
                .Select(g => g.OrderByDescending(c => c.Pontuacao).First())
                .ToList()
            : todasCapturas;

        if (vm.QuantidadeEquipes > 0)
        {
            var equipes = (await _equipeServico.ListarTodos()).ToList();
            vm.Equipes.AddRange(equipes
                .Select(e => new GanhadorRelatorioViewModel
                {
                    EquipeId        = e.Id,
                    NomeEquipe      = e.Nome,
                    Capitao         = e.Capitao,
                    TotalPontos     = capturasPontuacao.Where(c => c.EquipeId == e.Id).Sum(c => c.Pontuacao),
                    PrimeiraCaptura = capturasPontuacao.Where(c => c.EquipeId == e.Id).Select(c => c.DataHora).DefaultIfEmpty(DateTime.MaxValue).Min()
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.PrimeiraCaptura).ThenBy(x => x.NomeEquipe)
                .Take(vm.QuantidadeEquipes)
                .Select((x, i) => new GanhadorRelatorioViewModel
                {
                    Posicao = i + 1, EquipeId = x.EquipeId,
                    NomeEquipe = x.NomeEquipe, Capitao = x.Capitao,
                    TotalPontos = x.TotalPontos
                }));
        }

        if (vm.QuantidadeMembrosPontuacao > 0)
        {
            var membros = (await _membroServico.ListarTodos()).ToList();
            vm.MembrosPontuacao.AddRange(membros
                .Select(m => new GanhadorRelatorioViewModel
                {
                    MembroId        = m.Id,
                    NomeMembro      = m.Nome,
                    TotalPontos     = capturasPontuacao.Where(c => c.MembroId == m.Id).Sum(c => c.Pontuacao),
                    PrimeiraCaptura = capturasPontuacao.Where(c => c.MembroId == m.Id).Select(c => c.DataHora).DefaultIfEmpty(DateTime.MaxValue).Min()
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.PrimeiraCaptura).ThenBy(x => x.NomeMembro)
                .Take(vm.QuantidadeMembrosPontuacao)
                .Select((x, i) => new GanhadorRelatorioViewModel
                {
                    Posicao = i + 1, MembroId = x.MembroId,
                    NomeMembro = x.NomeMembro, TotalPontos = x.TotalPontos
                }));
        }

        if (vm.ExibirMaiorCaptura && vm.QuantidadeMembrosMaiorCaptura > 0)
        {
            vm.MembrosMaiorCaptura.AddRange(todasCapturas
                .GroupBy(c => c.MembroId)
                .Select(g =>
                {
                    var maior = g
                        .OrderByDescending(c => c.TamanhoMedida)
                        .ThenBy(c => c.DataHora)
                        .First();
                    return new GanhadorRelatorioViewModel
                    {
                        MembroId = maior.MembroId,
                        NomeMembro = maior.NomeMembro,
                        MaiorCaptura = maior.TamanhoMedida,
                        NomeItemMaiorCaptura = maior.NomeItem,
                        PrimeiraCaptura = maior.DataHora
                    };
                })
                .OrderByDescending(x => x.MaiorCaptura ?? 0)
                .ThenBy(x => x.PrimeiraCaptura)
                .ThenBy(x => x.NomeMembro)
                .Take(vm.QuantidadeMembrosMaiorCaptura)
                .Select((x, i) => new GanhadorRelatorioViewModel
                {
                    Posicao = i + 1,
                    MembroId = x.MembroId,
                    NomeMembro = x.NomeMembro,
                    MaiorCaptura = x.MaiorCaptura,
                    NomeItemMaiorCaptura = x.NomeItemMaiorCaptura,
                    PrimeiraCaptura = x.PrimeiraCaptura
                }));
        }

        ViewBag.Torneio = torneio;
        return View(vm);
    }

    [HttpGet("ganhadores/download")]
    public async Task<IActionResult> DownloadGanhadores(
        [FromQuery] int quantidadeEquipes = 3,
        [FromQuery] int quantidadeMembrosPontuacao = 3,
        [FromQuery] int quantidadeMembrosMaiorCaptura = 3,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _relatorioServico.GerarRelatorioGanhadores(
                quantidadeEquipes,
                quantidadeMembrosPontuacao,
                quantidadeMembrosMaiorCaptura,
                analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"ganhadores_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(SelecionarGanhadores), new
            {
                slug = Slug,
                quantidadeEquipes,
                quantidadeMembrosPontuacao,
                quantidadeMembrosMaiorCaptura
            });
        }
    }

    [HttpGet("maiores-capturas")]
    public async Task<IActionResult> MaioresCapturas()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (torneio.TipoTorneio != nameof(TipoTorneio.Pesca))
        {
            TempData["Erro"] = "O relatório de maiores capturas está disponível somente para torneios do tipo pesca.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }

        ViewBag.Torneio = torneio;
        return View(new MaioresCapturasFiltroViewModel());
    }

    [HttpGet("maiores-capturas/download")]
    public async Task<IActionResult> DownloadMaioresCapturas([FromQuery] int quantidade = 1)
    {
        try
        {
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            if (torneio is null) return NotFound();
            if (torneio.TipoTorneio != nameof(TipoTorneio.Pesca))
            {
                TempData["Erro"] = "O relatório de maiores capturas está disponível somente para torneios do tipo pesca.";
                return RedirectToAction(nameof(Index), new { slug = Slug });
            }

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
