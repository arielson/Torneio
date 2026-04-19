using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;
using System.Text;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/sorteio")]
public class SorteioController : TorneioBaseController
{
    private readonly ISorteioAppServico _servico;
    private readonly ITorneioServico _torneioServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly IWebHostEnvironment _env;
    private readonly ILogAuditoriaServico _log;

    public SorteioController(
        TenantContext tenantContext,
        ISorteioAppServico servico,
        ITorneioServico torneioServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        IWebHostEnvironment env,
        ILogAuditoriaServico log)
        : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _env = env;
        _log = log;
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
        ViewBag.IsDevelopment = _env.IsDevelopment();

        var equipes = await _equipeServico.ListarTodos();
        var membros = await _membroServico.ListarTodos();
        ViewBag.NomesEquipes = equipes.Select(e => e.Nome).ToList();
        ViewBag.NomesMembros = membros.Select(m => m.Nome).ToList();

        var resultado = await _servico.ObterResultado();
        return View(resultado);
    }

    [HttpPost("realizar-ajax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RealizarAjax()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return Json(new { ok = false, erro = "Torneio não encontrado." });
        if (string.Equals(torneio.ModoSorteio, "Nenhum", StringComparison.OrdinalIgnoreCase))
            return Json(new { ok = false, erro = "Este torneio não utiliza sorteio." });
        try
        {
            // Apenas calcula — não salva ainda. O save acontece em /confirmar-ajax.
            var resultado = await _servico.RealizarSorteio();
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio.NomeTorneio,
                Categoria = CategoriaLog.Sorteio, Acao = "SorteioCalculado",
                Descricao = $"Sorteio calculado ({resultado.Count()} {torneio.LabelMembroPlural.ToLower()} distribuídos). Aguardando confirmação.",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            return Json(new { ok = true, resultado = resultado.OrderBy(x => x.Posicao) });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, erro = ex.Message });
        }
    }

    [HttpPost("confirmar-ajax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarAjax([FromBody] List<ConfirmarSorteioItemDto> itens)
    {
        try
        {
            await _servico.ConfirmarSorteio(itens);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            var resumo = BuildSorteioResumo(itens);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Sorteio, Acao = "SorteioConfirmado",
                Descricao = $"Sorteio confirmado: {itens.Count} {(torneio?.LabelMembroPlural ?? "membros").ToLower()} | {resumo}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, erro = ex.Message });
        }
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

        var resultadoAnterior = await _servico.ObterResultado();
        var resumoAnterior = BuildSorteioResumo(resultadoAnterior
            .Select(r => new ConfirmarSorteioItemDto { NomeEquipe = r.NomeEquipe, NomeMembro = r.NomeMembro, Posicao = r.Posicao }));
        await _servico.LimparSorteio();
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioLimpo",
            Descricao = $"Resultado do sorteio removido | {resumoAnterior}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
        });
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
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio.NomeTorneio,
                Categoria = CategoriaLog.Sorteio, Acao = "PosicaoAjustada",
                Descricao = $"Posição do sorteio ajustada para {novaPosicao} (id: {sorteioEquipeId}).",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            TempData["Sucesso"] = "Posição ajustada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    private static string BuildSorteioResumo(IEnumerable<ConfirmarSorteioItemDto> itens)
    {
        var grupos = itens
            .GroupBy(i => i.NomeEquipe)
            .Select(g => $"{g.Key}: {string.Join(", ", g.OrderBy(i => i.Posicao).Select(i => i.NomeMembro))}");
        return string.Join(" | ", grupos);
    }
}
