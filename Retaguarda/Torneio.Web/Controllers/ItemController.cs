using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Item;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/itens")]
public class ItemController : TorneioBaseController
{
    private readonly IItemServico _servico;
    private readonly ITorneioServico _torneioServico;

    public ItemController(TenantContext tenantContext, IItemServico servico, ITorneioServico torneioServico) : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
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
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
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
        await _servico.Remover(id);
        TempData["Sucesso"] = "Item removido.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
