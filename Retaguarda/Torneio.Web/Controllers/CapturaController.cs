using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Infrastructure.Services;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/capturas")]
public class CapturaController : TorneioBaseController
{
    private readonly ICapturaServico _capturaServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly IItemServico _itemServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public CapturaController(
        TenantContext tenantContext,
        ICapturaServico capturaServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        IItemServico itemServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log)
        : base(tenantContext)
    {
        _capturaServico = capturaServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _itemServico = itemServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var capturas = await _capturaServico.ListarTodos();
        return View(capturas);
    }

    [HttpGet("registrar")]
    public async Task<IActionResult> Registrar()
    {
        await CarregarSelectsAsync();
        return View(new RegistrarCapturaWebDto { DataHora = DateTime.Now });
    }

    [HttpPost("registrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarCapturaWebDto form, IFormFile? foto)
    {
        if (!ModelState.IsValid)
        {
            await CarregarSelectsAsync();
            return View(form);
        }

        try
        {
            var fotoPath = await SalvarFotoAsync(foto, "capturas");

            var dto = new RegistrarCapturaDto
            {
                TorneioId = TenantContext.TorneioId,
                EquipeId  = form.EquipeId,
                MembroId  = form.MembroId,
                ItemId    = form.ItemId,
                TamanhoMedida = form.TamanhoMedida,
                FotoUrl   = fotoPath,
                DataHora  = form.DataHora.ToUniversalTime(),
                Origem    = OrigemCaptura.Retaguarda
            };

            var criada = await _capturaServico.Registrar(dto);
            TempData["Sucesso"] = "Captura registrada com sucesso.";
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Capturas, Acao = "RegistrarCapturaRetaguarda",
                Descricao = $"Captura registrada | Item: {criada.NomeItem} | Pescador: {criada.NomeMembro} | Equipe: {criada.NomeEquipe} | Medida: {criada.TamanhoMedida} | Data: {criada.DataHora:dd/MM/yyyy HH:mm}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CarregarSelectsAsync();
            return View(form);
        }
    }

    [HttpPost("{id:guid}/invalidar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invalidar(Guid id, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            TempData["Erro"] = "Informe o motivo da invalidação.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }

        try
        {
            var captura = await _capturaServico.ObterPorId(id);
            await _capturaServico.Invalidar(id, motivo.Trim());
            TempData["Sucesso"] = "Captura invalidada.";
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Capturas, Acao = "InvalidarCaptura",
                Descricao = $"Captura invalidada | Item: {captura?.NomeItem} | Pescador: {captura?.NomeMembro} | Equipe: {captura?.NomeEquipe} | Medida: {captura?.TamanhoMedida} | Motivo: {motivo.Trim()}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/revalidar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revalidar(Guid id)
    {
        try
        {
            var captura = await _capturaServico.ObterPorId(id);
            await _capturaServico.Revalidar(id);
            TempData["Sucesso"] = "Captura revalidada.";
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Capturas, Acao = "RevalidarCaptura",
                Descricao = $"Captura revalidada | Item: {captura?.NomeItem} | Pescador: {captura?.NomeMembro} | Equipe: {captura?.NomeEquipe} | Medida: {captura?.TamanhoMedida}",
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
        var captura = await _capturaServico.ObterPorId(id);
        await _capturaServico.Remover(id);
        TempData["Sucesso"] = "Captura removida.";
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Capturas, Acao = "RemoverCaptura",
            Descricao = $"Captura removida | Item: {captura?.NomeItem ?? id.ToString()} | Pescador: {captura?.NomeMembro} | Equipe: {captura?.NomeEquipe} | Medida: {captura?.TamanhoMedida} | Data: {captura?.DataHora:dd/MM/yyyy HH:mm}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    private async Task CarregarSelectsAsync()
    {
        ViewBag.Equipes  = await _equipeServico.ListarTodos();
        ViewBag.Membros  = await _membroServico.ListarTodos();
        ViewBag.Itens    = await _itemServico.ListarPorTorneio(TenantContext.TorneioId);
    }
}
