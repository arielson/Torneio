using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/admin")]
public class TorneioAdminController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _logAuditoriaServico;

    public TorneioAdminController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        ILogAuditoriaServico logAuditoriaServico)
        : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _logAuditoriaServico = logAuditoriaServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        return View(torneio);
    }

    [HttpPost("liberar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar()
    {
        try
        {
            await _torneioServico.Liberar(TenantContext.TorneioId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "LiberarTorneio",
                Descricao = "Status do torneio alterado para Liberado via retaguarda web.",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Torneio liberado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("finalizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar()
    {
        try
        {
            await _torneioServico.Finalizar(TenantContext.TorneioId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "FinalizarTorneio",
                Descricao = "Status do torneio alterado para Finalizado via retaguarda web.",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Torneio finalizado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("reabrir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reabrir()
    {
        try
        {
            await _torneioServico.Reabrir(TenantContext.TorneioId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "ReabrirTorneio",
                Descricao = "Status do torneio alterado para Aberto via retaguarda web.",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Torneio reaberto.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("clonar")]
    public IActionResult Clonar() => View();

    [HttpPost("clonar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clonar(string novoSlug, string novoNome)
    {
        try
        {
            var novo = await _torneioServico.ClonarTorneio(TenantContext.TorneioId, novoSlug, novoNome);
            TempData["Sucesso"] = $"Torneio \"{novo.NomeTorneio}\" criado a partir desta edição.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            return View();
        }
    }
}
