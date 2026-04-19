using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Premio;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/premios")]
public class PremioController : TorneioBaseController
{
    private readonly IPremioServico _premioServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public PremioController(TenantContext tenantContext, IPremioServico premioServico, ITorneioServico torneioServico, ILogAuditoriaServico log)
        : base(tenantContext)
    {
        _premioServico = premioServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        ViewBag.Torneio = torneio;
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
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Premios, Acao = "CriarPremio",
                Descricao = $"Prêmio adicionado: {dto.Posicao}º lugar — {dto.Descricao}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
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
        var premios = await _premioServico.ListarPorTorneio(TenantContext.TorneioId);
        var premio = premios.FirstOrDefault(p => p.Id == id);
        await _premioServico.Remover(id);
        TempData["Sucesso"] = "Prêmio removido.";
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Premios, Acao = "RemoverPremio",
            Descricao = $"Prêmio removido | Posição: {premio?.Posicao}º lugar | Descrição: {premio?.Descricao ?? id.ToString()}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
