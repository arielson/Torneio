using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/equipes")]
public class EquipeController : TorneioBaseController
{
    private readonly IEquipeServico _servico;
    private readonly IFiscalServico _fiscalServico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;

    public EquipeController(
        TenantContext tenantContext,
        IEquipeServico servico,
        IFiscalServico fiscalServico,
        IMembroServico membroServico,
        ITorneioServico torneioServico) : base(tenantContext)
    {
        _servico = servico;
        _fiscalServico = fiscalServico;
        _membroServico = membroServico;
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
        var equipes = await _servico.ListarTodos();
        return View(equipes);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        ViewBag.Fiscais = await _fiscalServico.ListarTodos();
        await SetTorneioViewBag();
        return View(new CriarEquipeDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEquipeDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is not null && string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
        {
            dto = new CriarEquipeDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                FiscalId = dto.FiscalId,
                QtdVagas = 1,
                FotoUrl = dto.FotoUrl,
                FotoCapitaoUrl = dto.FotoCapitaoUrl,
            };
        }

        if (dto.FiscalId == Guid.Empty)
            ModelState.AddModelError(nameof(dto.FiscalId), "Selecione um fiscal.");

        if (!ModelState.IsValid)
        {
            ViewBag.Fiscais = await _fiscalServico.ListarTodos();
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
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                FiscalId = dto.FiscalId,
                QtdVagas = dto.QtdVagas,
                FotoUrl = fotoUrl,
                FotoCapitaoUrl = fotoCapitaoUrl,
            });
            TempData["Sucesso"] = "Equipe criada com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Fiscais = await _fiscalServico.ListarTodos();
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var equipe = await _servico.ObterPorId(id);
        if (equipe is null) return NotFound();
        ViewBag.Equipe = equipe;
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
    public async Task<IActionResult> Editar(Guid id, AtualizarEquipeDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is not null && string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
        {
            dto = new AtualizarEquipeDto
            {
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = 1,
                FotoUrl = dto.FotoUrl,
                FotoCapitaoUrl = dto.FotoCapitaoUrl,
            };
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Equipe = await _servico.ObterPorId(id);
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
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Equipe = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        TempData["Sucesso"] = "Equipe removida.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("{id:guid}/membros")]
    public async Task<IActionResult> Membros(Guid id)
    {
        var equipe = await _servico.ObterPorId(id);
        if (equipe is null) return NotFound();
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        ViewBag.Equipe = equipe;
        ViewBag.Torneio = torneio;

        var todosMembros = await _membroServico.ListarTodos();
        return View(todosMembros);
    }

    [HttpPost("{id:guid}/membros/{membroId:guid}/adicionar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarMembro(Guid id, Guid membroId)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        try
        {
            await _servico.AdicionarMembro(id, membroId);
            TempData["Sucesso"] = "Membro adicionado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Membros), new { slug = Slug, id });
    }

    [HttpPost("{id:guid}/membros/{membroId:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverMembro(Guid id, Guid membroId)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        await _servico.RemoverMembro(id, membroId);
        TempData["Sucesso"] = "Membro removido da equipe.";
        return RedirectToAction(nameof(Membros), new { slug = Slug, id });
    }
}
