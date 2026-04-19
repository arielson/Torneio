using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Item;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/itens")]
public class ItemController : TorneioBaseController
{
    private readonly IItemServico _servico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public ItemController(TenantContext tenantContext, IItemServico servico, ITorneioServico torneioServico, ILogAuditoriaServico log) : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private async Task SetTorneioViewBag()
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetTorneioViewBag();
        var itens = await _servico.ListarPorTorneio(TenantContext.TorneioId);
        return View(itens);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetTorneioViewBag();
        return View(new CriarItemDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarItemDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is not null && !torneio.UsarFatorMultiplicador)
        {
            dto = new CriarItemDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = 1.0m,
                FotoUrl = dto.FotoUrl,
            };
        }

        if (!ModelState.IsValid)
        {
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/itens");
            await _servico.Criar(new CriarItemDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = dto.FatorMultiplicador,
                FotoUrl = fotoUrl,
            });
            TempData["Sucesso"] = "Item criado com sucesso.";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Itens, Acao = "CriarItem",
                Descricao = $"Item criado | Nome: {dto.Nome} | Comprimento mínimo: {dto.Comprimento} | Fator: {dto.FatorMultiplicador}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var item = await _servico.ObterPorId(id);
        if (item is null) return NotFound();
        ViewBag.Item = item;
        await SetTorneioViewBag();
        return View(new AtualizarItemDto
        {
            Nome = item.Nome,
            Comprimento = item.Comprimento,
            FatorMultiplicador = item.FatorMultiplicador,
            FotoUrl = item.FotoUrl,
        });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarItemDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is not null && !torneio.UsarFatorMultiplicador)
        {
            dto = new AtualizarItemDto
            {
                Nome = dto.Nome,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = 1.0m,
                FotoUrl = dto.FotoUrl,
            };
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Item = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var itemAtual = await _servico.ObterPorId(id);
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/itens") ?? itemAtual?.FotoUrl;
            dto = new AtualizarItemDto
            {
                Nome = dto.Nome,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = dto.FatorMultiplicador,
                FotoUrl = fotoUrl,
            };
            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Item atualizado.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Item = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        var item = await _servico.ObterPorId(id);
        await _servico.Remover(id);
        TempData["Sucesso"] = "Item removido.";
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Itens, Acao = "RemoverItem",
            Descricao = $"Item removido | Nome: {item?.Nome ?? id.ToString()} | Comprimento mínimo: {item?.Comprimento} | Fator: {item?.FatorMultiplicador}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
