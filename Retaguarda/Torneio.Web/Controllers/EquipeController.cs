using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/equipes/{anoId:guid}")]
public class EquipeController : TorneioBaseController
{
    private readonly IEquipeServico _servico;
    private readonly IAnoTorneioServico _anoServico;
    private readonly IFiscalServico _fiscalServico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;

    public EquipeController(
        TenantContext tenantContext,
        IEquipeServico servico,
        IAnoTorneioServico anoServico,
        IFiscalServico fiscalServico,
        IMembroServico membroServico,
        ITorneioServico torneioServico) : base(tenantContext)
    {
        _servico = servico;
        _anoServico = anoServico;
        _fiscalServico = fiscalServico;
        _membroServico = membroServico;
        _torneioServico = torneioServico;
    }

    private async Task SetTorneioViewBag()
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid anoId)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;
        await SetTorneioViewBag();

        var equipes = await _servico.ListarPorAnoTorneio(anoId);
        return View(equipes);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar(Guid anoId)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;
        ViewBag.Fiscais = await _fiscalServico.ListarPorAnoTorneio(anoId);
        await SetTorneioViewBag();
        return View(new CriarEquipeDto { TorneioId = TenantContext.TorneioId, AnoTorneioId = anoId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(Guid anoId, CriarEquipeDto dto)
    {
        var ano = await _anoServico.ObterPorId(anoId);
        if (ano is null) return NotFound();
        ViewBag.Ano = ano;

        if (dto.FiscalId == Guid.Empty)
            ModelState.AddModelError(nameof(dto.FiscalId), "Selecione um fiscal.");

        if (!ModelState.IsValid)
        {
            ViewBag.Fiscais = await _fiscalServico.ListarPorAnoTorneio(anoId);
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/equipes");
            var fotoCapitaoUrl = await SalvarFotoAsync(Request.Form.Files["fotoCapitao"], "fotos/capitaos");

            await _servico.Criar(new CriarEquipeDto
            {
                TorneioId = TenantContext.TorneioId,
                AnoTorneioId = anoId,
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                FiscalId = dto.FiscalId,
                QtdVagas = dto.QtdVagas,
                FotoUrl = fotoUrl,
                FotoCapitaoUrl = fotoCapitaoUrl,
            });
            TempData["Sucesso"] = "Equipe criada com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug, anoId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Fiscais = await _fiscalServico.ListarPorAnoTorneio(anoId);
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid anoId, Guid id)
    {
        var equipe = await _servico.ObterPorId(id);
        if (equipe is null) return NotFound();
        ViewBag.Equipe = equipe;
        ViewBag.AnoId = anoId;
        await SetTorneioViewBag();

        var dto = new AtualizarEquipeDto
        {
            Nome = equipe.Nome,
            Capitao = equipe.Capitao,
            QtdVagas = equipe.QtdVagas,
            FotoUrl = equipe.FotoUrl,
            FotoCapitaoUrl = equipe.FotoCapitaoUrl,
        };
        return View(dto);
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid anoId, Guid id, AtualizarEquipeDto dto)
    {
        if (!ModelState.IsValid)
        {
            var equipe = await _servico.ObterPorId(id);
            ViewBag.Equipe = equipe;
            ViewBag.AnoId = anoId;
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var equipeAtual = await _servico.ObterPorId(id);
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/equipes") ?? equipeAtual?.FotoUrl;
            var fotoCapitaoUrl = await SalvarFotoAsync(Request.Form.Files["fotoCapitao"], "fotos/capitaos") ?? equipeAtual?.FotoCapitaoUrl;

            dto = new AtualizarEquipeDto
            {
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = dto.QtdVagas,
                FotoUrl = fotoUrl,
                FotoCapitaoUrl = fotoCapitaoUrl,
            };

            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Equipe atualizada.";
            return RedirectToAction(nameof(Index), new { slug = Slug, anoId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var equipe = await _servico.ObterPorId(id);
            ViewBag.Equipe = equipe;
            ViewBag.AnoId = anoId;
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid anoId, Guid id)
    {
        await _servico.Remover(id);
        TempData["Sucesso"] = "Equipe removida.";
        return RedirectToAction(nameof(Index), new { slug = Slug, anoId });
    }

    [HttpGet("{id:guid}/membros")]
    public async Task<IActionResult> Membros(Guid anoId, Guid id)
    {
        var equipe = await _servico.ObterPorId(id);
        if (equipe is null) return NotFound();
        ViewBag.Equipe = equipe;
        ViewBag.AnoId = anoId;
        await SetTorneioViewBag();

        var todosMembros = await _membroServico.ListarPorAnoTorneio(anoId);
        return View(todosMembros);
    }

    [HttpPost("{id:guid}/membros/{membroId:guid}/adicionar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarMembro(Guid anoId, Guid id, Guid membroId)
    {
        try
        {
            await _servico.AdicionarMembro(id, membroId);
            TempData["Sucesso"] = "Membro adicionado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Membros), new { slug = Slug, anoId, id });
    }

    [HttpPost("{id:guid}/membros/{membroId:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverMembro(Guid anoId, Guid id, Guid membroId)
    {
        await _servico.RemoverMembro(id, membroId);
        TempData["Sucesso"] = "Membro removido da equipe.";
        return RedirectToAction(nameof(Membros), new { slug = Slug, anoId, id });
    }
}
