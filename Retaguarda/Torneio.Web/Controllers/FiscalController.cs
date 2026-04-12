using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/fiscais/{anoId:guid}")]
public class FiscalController : TorneioBaseController
{
    private readonly IFiscalServico _servico;
    private readonly IAnoTorneioServico _anoServico;

    public FiscalController(TenantContext tenantContext, IFiscalServico servico, IAnoTorneioServico anoServico)
        : base(tenantContext)
    {
        _servico = servico;
        _anoServico = anoServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid anoId)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;

        var fiscais = await _servico.ListarPorAnoTorneio(anoId);
        return View(fiscais);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar(Guid anoId)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;
        return View(new CriarFiscalDto { TorneioId = TenantContext.TorneioId, AnoTorneioId = anoId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(Guid anoId, CriarFiscalDto dto)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;

        if (!ModelState.IsValid) return View(dto);
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/fiscais");
            await _servico.Criar(new CriarFiscalDto
            {
                TorneioId = TenantContext.TorneioId,
                AnoTorneioId = anoId,
                Nome = dto.Nome,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
                FotoUrl = fotoUrl,
            });
            TempData["Sucesso"] = "Fiscal criado com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug, anoId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid anoId, Guid id)
    {
        await _servico.Remover(id);
        TempData["Sucesso"] = "Fiscal removido.";
        return RedirectToAction(nameof(Index), new { slug = Slug, anoId });
    }

    [HttpGet("{id:guid}/senha")]
    public async Task<IActionResult> AlterarSenha(Guid anoId, Guid id)
    {
        var fiscal = await _servico.ObterPorId(id);
        if (fiscal is null) return NotFound();
        ViewBag.Fiscal = fiscal;
        ViewBag.AnoId = anoId;
        return View(new AtualizarSenhaDto());
    }

    [HttpPost("{id:guid}/senha")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(Guid anoId, Guid id, AtualizarSenhaDto dto)
    {
        if (!ModelState.IsValid)
        {
            var fiscal = await _servico.ObterPorId(id);
            ViewBag.Fiscal = fiscal;
            ViewBag.AnoId = anoId;
            return View(dto);
        }
        try
        {
            await _servico.AtualizarSenha(id, dto);
            TempData["Sucesso"] = "Senha alterada.";
            return RedirectToAction(nameof(Index), new { slug = Slug, anoId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
}
