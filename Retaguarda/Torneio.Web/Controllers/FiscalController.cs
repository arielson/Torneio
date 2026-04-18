using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/fiscais")]
public class FiscalController : TorneioBaseController
{
    private readonly IFiscalServico _servico;
    private readonly ITorneioServico _torneioServico;

    public FiscalController(TenantContext tenantContext, IFiscalServico servico, ITorneioServico torneioServico)
        : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
    }

    private async Task SetTorneioViewBag()
        => ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetTorneioViewBag();
        var fiscais = await _servico.ListarTodos();
        return View(fiscais);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetTorneioViewBag();
        return View(new CriarFiscalDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarFiscalDto dto)
    {
        if (!ModelState.IsValid)
        {
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/fiscais");
            await _servico.Criar(new CriarFiscalDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
                FotoUrl = fotoUrl,
            });
            TempData["Sucesso"] = "Fiscal criado com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            await SetTorneioViewBag();
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        TempData["Sucesso"] = "Fiscal removido.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("{id:guid}/senha")]
    public async Task<IActionResult> AlterarSenha(Guid id)
    {
        var fiscal = await _servico.ObterPorId(id);
        if (fiscal is null) return NotFound();
        await SetTorneioViewBag();
        ViewBag.Fiscal = fiscal;
        return View(new AtualizarSenhaDto());
    }

    [HttpPost("{id:guid}/senha")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(Guid id, AtualizarSenhaDto dto)
    {
        if (!ModelState.IsValid)
        {
            var fiscal = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            ViewBag.Fiscal = fiscal;
            return View(dto);
        }
        try
        {
            await _servico.AtualizarSenha(id, dto);
            TempData["Sucesso"] = "Senha alterada.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            var fiscal = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            ViewBag.Fiscal = fiscal;
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
}
