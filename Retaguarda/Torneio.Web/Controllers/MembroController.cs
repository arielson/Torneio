using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/membros")]
public class MembroController : TorneioBaseController
{
    private readonly IMembroServico _servico;
    private readonly ITorneioServico _torneioServico;

    public MembroController(TenantContext tenantContext, IMembroServico servico, ITorneioServico torneioServico)
        : base(tenantContext)
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
        var membros = await _servico.ListarTodos();
        return View(membros);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetTorneioViewBag();
        return View(new CriarMembroDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarMembroDto dto)
    {
        if (!ModelState.IsValid)
        {
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/membros");
            await _servico.Criar(new CriarMembroDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                FotoUrl = fotoUrl,
            });
            TempData["Sucesso"] = "Membro criado com sucesso.";
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
        var membro = await _servico.ObterPorId(id);
        if (membro is null) return NotFound();
        ViewBag.Membro = membro;
        await SetTorneioViewBag();
        return View(new AtualizarMembroDto { Nome = membro.Nome, FotoUrl = membro.FotoUrl });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarMembroDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Membro = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var membroAtual = await _servico.ObterPorId(id);
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/membros") ?? membroAtual?.FotoUrl;
            dto = new AtualizarMembroDto { Nome = dto.Nome, FotoUrl = fotoUrl };
            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Membro atualizado.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Membro = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        TempData["Sucesso"] = "Membro removido.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
