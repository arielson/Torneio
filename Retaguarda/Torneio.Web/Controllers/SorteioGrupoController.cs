using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/sorteio-grupo")]
public class SorteioGrupoController : TorneioBaseController
{
    private readonly ISorteioGrupoAppServico _servico;
    private readonly IGrupoAppServico _grupoServico;
    private readonly IEquipeServico _equipeServico;
    private readonly ITorneioServico _torneioServico;
    private readonly IWebHostEnvironment _env;
    private readonly ILogAuditoriaServico _log;

    public SorteioGrupoController(
        TenantContext tenantContext,
        ISorteioGrupoAppServico servico,
        IGrupoAppServico grupoServico,
        IEquipeServico equipeServico,
        ITorneioServico torneioServico,
        IWebHostEnvironment env,
        ILogAuditoriaServico log) : base(tenantContext)
    {
        _servico = servico;
        _grupoServico = grupoServico;
        _equipeServico = equipeServico;
        _torneioServico = torneioServico;
        _env = env;
        _log = log;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        ViewBag.Torneio = torneio;
        ViewBag.PreCondicoes = await _servico.VerificarPreCondicoes();
        ViewBag.IsDevelopment = _env.IsDevelopment();

        var grupos = (await _grupoServico.ListarTodos()).OrderBy(g => g.Nome).ToList();
        var equipes = (await _equipeServico.ListarTodos()).OrderBy(e => e.Nome).ToList();
        ViewBag.Grupos = grupos;
        ViewBag.NomesGrupos = grupos.Select(g => g.Nome).ToList();
        ViewBag.Equipes = equipes;
        ViewBag.NomesEquipes = equipes.Select(e => e.Nome).ToList();

        var resultado = await _servico.ObterResultado();
        return View(resultado);
    }

    [HttpPost("realizar-ajax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RealizarAjax([FromBody] RealizarSorteioGrupoDto? filtro = null)
    {
        try
        {
            var resultado = await _servico.RealizarSorteio(filtro);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            var lista = resultado.ToList();
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Sorteio, Acao = "SorteioGrupoCalculado",
                Descricao = $"Sorteio de grupos calculado ({lista.Count} grupos distribuídos). Aguardando confirmação.",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            return Json(new { ok = true, resultado = lista.OrderBy(r => r.Posicao) });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, erro = ex.Message });
        }
    }

    [HttpPost("confirmar-ajax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarAjax([FromBody] List<ConfirmarSorteioGrupoItemDto> itens)
    {
        try
        {
            await _servico.ConfirmarSorteio(itens);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            var resumo = string.Join(" | ", itens.Select(i => $"{i.NomeGrupo} → {i.NomeEquipe}"));
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Sorteio, Acao = "SorteioGrupoConfirmado",
                Descricao = $"Sorteio de grupos confirmado: {itens.Count} grupos | {resumo}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, erro = ex.Message });
        }
    }

    [HttpPost("limpar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Limpar()
    {
        var resultadoAnterior = (await _servico.ObterResultado()).ToList();
        var resumo = string.Join(" | ", resultadoAnterior.Select(r => $"{r.NomeGrupo} → {r.NomeEquipe}"));
        await _servico.LimparSorteio();
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioGrupoLimpo",
            Descricao = $"Sorteio de grupos removido | {(string.IsNullOrEmpty(resumo) ? "vazio" : resumo)}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
        });
        TempData["Sucesso"] = "Sorteio limpo.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
