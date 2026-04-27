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
    private readonly IEspeciePeixeServico _especieServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public ItemController(
        TenantContext tenantContext,
        IItemServico servico,
        IEspeciePeixeServico especieServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log) : base(tenantContext)
    {
        _servico = servico;
        _especieServico = especieServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private async Task SetViewBag(Guid? itemAtualId = null)
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        ViewBag.Especies = await _especieServico.ListarTodas();
        var itensDoTorneio = await _servico.ListarPorTorneio(TenantContext.TorneioId);
        // Espécies já adicionadas, excluindo o item que está sendo editado
        ViewBag.EspeciesJaAdicionadas = itensDoTorneio
            .Where(i => i.Id != itemAtualId)
            .Select(i => i.EspeciePeixeId)
            .ToHashSet();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetViewBag();
        var itens = await _servico.ListarPorTorneio(TenantContext.TorneioId);
        return View(itens);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetViewBag();
        return View(new CriarItemDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarItemDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));

        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is not null && !torneio.UsarFatorMultiplicador)
        {
            dto = new CriarItemDto
            {
                TorneioId = TenantContext.TorneioId,
                EspeciePeixeId = dto.EspeciePeixeId,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = 1.0m,
            };
        }

        if (!ModelState.IsValid || dto.EspeciePeixeId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(dto.EspeciePeixeId), "Selecione uma espécie.");
            await SetViewBag();
            return View(dto);
        }

        try
        {
            var especie = await _especieServico.ObterPorId(dto.EspeciePeixeId);
            await _servico.Criar(new CriarItemDto
            {
                TorneioId = TenantContext.TorneioId,
                EspeciePeixeId = dto.EspeciePeixeId,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = dto.FatorMultiplicador,
            });
            TempData["Sucesso"] = "Item adicionado ao torneio.";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Itens, Acao = "CriarItem",
                Descricao = $"Item adicionado | Espécie: {especie?.Nome} | Comprimento mínimo: {dto.Comprimento} | Fator: {dto.FatorMultiplicador}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await SetViewBag();
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var item = await _servico.ObterPorId(id);
        if (item is null) return NotFound();
        ViewBag.Item = item;
        await SetViewBag(itemAtualId: id);
        return View(new AtualizarItemDto
        {
            EspeciePeixeId = item.EspeciePeixeId,
            Comprimento = item.Comprimento,
            FatorMultiplicador = item.FatorMultiplicador,
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
                EspeciePeixeId = dto.EspeciePeixeId,
                Comprimento = dto.Comprimento,
                FatorMultiplicador = 1.0m,
            };
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Item = await _servico.ObterPorId(id);
            await SetViewBag(itemAtualId: id);
            return View(dto);
        }
        try
        {
            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Item atualizado.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Item = await _servico.ObterPorId(id);
            await SetViewBag(itemAtualId: id);
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
            Descricao = $"Item removido | Espécie: {item?.Nome ?? id.ToString()} | Comprimento mínimo: {item?.Comprimento} | Fator: {item?.FatorMultiplicador}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
