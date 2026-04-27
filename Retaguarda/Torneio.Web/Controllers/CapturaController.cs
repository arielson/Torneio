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

    [HttpPost("{id:guid}/alterar-tamanho")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarTamanho(Guid id, decimal tamanhoMedida)
    {
        try
        {
            var capturaAntes = await _capturaServico.ObterPorId(id);
            await _capturaServico.AlterarTamanho(id, tamanhoMedida);
            var capturaDepois = await _capturaServico.ObterPorId(id);
            TempData["Sucesso"] = "Tamanho da captura atualizado.";
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Capturas,
                Acao = "AlterarTamanhoCapturaRetaguarda",
                Descricao = $"Tamanho da captura alterado | Item: {capturaAntes?.NomeItem} | Pescador: {capturaAntes?.NomeMembro} | Equipe: {capturaAntes?.NomeEquipe} | Medida anterior: {capturaAntes?.TamanhoMedida} | Nova medida: {capturaDepois?.TamanhoMedida ?? tamanhoMedida}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/editar-completo")]
    [Authorize(Policy = "AdminGeral")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCompleto(Guid id, decimal tamanhoMedida, DateTime dataHora, IFormFile? foto, bool removerFoto = false)
    {
        try
        {
            var capturaAntes = await _capturaServico.ObterPorId(id);
            if (capturaAntes is null)
            {
                TempData["Erro"] = "Captura não encontrada.";
                return RedirectToAction(nameof(Index), new { slug = Slug });
            }

            // Determina a nova foto:
            // - nova foto enviada  → salva e usa o novo path
            // - removerFoto=true   → null (sem foto)
            // - nenhum dos dois    → null sinaliza ao serviço "manter a atual"
            string? novaFotoPath = null;
            bool fotoAlterada = false;
            if (foto is { Length: > 0 })
            {
                novaFotoPath = await SalvarFotoAsync(foto, "capturas");
                fotoAlterada = true;
            }
            else if (removerFoto)
            {
                novaFotoPath = null;   // limpar
                fotoAlterada = true;
            }

            await _capturaServico.EditarCompleto(
                id, tamanhoMedida, novaFotoPath, dataHora.ToUniversalTime(),
                alterarFoto: fotoAlterada);

            // Apaga arquivo antigo quando a foto foi substituída ou removida
            if (fotoAlterada && !string.IsNullOrEmpty(capturaAntes.FotoUrl))
                await RemoverFotoAsync(capturaAntes.FotoUrl);

            TempData["Sucesso"] = "Captura atualizada.";
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);

            var alteracoes = new List<string>();
            if (capturaAntes.TamanhoMedida != tamanhoMedida)
                alteracoes.Add($"Medida: {capturaAntes.TamanhoMedida} → {tamanhoMedida}");
            if (capturaAntes.DataHora.ToLocalTime().ToString("dd/MM/yyyy HH:mm") != dataHora.ToString("dd/MM/yyyy HH:mm"))
                alteracoes.Add($"Data: {capturaAntes.DataHora.ToLocalTime():dd/MM/yyyy HH:mm} → {dataHora:dd/MM/yyyy HH:mm}");
            if (foto is { Length: > 0 })
                alteracoes.Add("Foto substituída");
            else if (removerFoto)
                alteracoes.Add("Foto removida");

            var descAlteracoes = alteracoes.Any() ? string.Join(" | ", alteracoes) : "Sem alterações";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Capturas, Acao = "EditarCapturaAdminGeral",
                Descricao = $"Captura editada (AdminGeral) | Item: {capturaAntes.NomeItem} | Pescador: {capturaAntes.NomeMembro} | Equipe: {capturaAntes.NomeEquipe} | {descAlteracoes}",
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
        await RemoverFotoAsync(captura?.FotoUrl);
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
