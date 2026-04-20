using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Grupo;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/grupos")]
public class GrupoController : TorneioBaseController
{
    private readonly IGrupoAppServico _servico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public GrupoController(
        TenantContext tenantContext,
        IGrupoAppServico servico,
        IMembroServico membroServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log) : base(tenantContext)
    {
        _servico = servico;
        _membroServico = membroServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private async Task SetViewBag()
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetViewBag();
        return View(await _servico.ListarTodos());
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetViewBag();
        return View(new CriarGrupoDto());
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarGrupoDto dto)
    {
        if (!ModelState.IsValid)
        {
            await SetViewBag();
            return View(dto);
        }
        try
        {
            var grupo = await _servico.Criar(dto);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Grupos, Acao = "GrupoCriado",
                Descricao = $"Grupo '{grupo.Nome}' criado.",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            TempData["Sucesso"] = $"Grupo '{grupo.Nome}' criado com sucesso.";
            return RedirectToAction(nameof(Detalhes), new { slug = Slug, id = grupo.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await SetViewBag();
            return View(dto);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detalhes(Guid id)
    {
        await SetViewBag();
        var grupo = await _servico.ObterPorId(id);
        if (grupo is null) return NotFound();

        var todosMembros = await _membroServico.ListarTodos();
        var membrosNoGrupo = grupo.Membros.Select(m => m.MembroId).ToHashSet();
        ViewBag.MembrosDisponiveis = todosMembros
            .Where(m => !membrosNoGrupo.Contains(m.Id))
            .OrderBy(m => m.Nome)
            .ToList();

        return View(grupo);
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        await SetViewBag();
        var grupo = await _servico.ObterPorId(id);
        if (grupo is null) return NotFound();
        return View(new AtualizarGrupoDto { Nome = grupo.Nome });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarGrupoDto dto)
    {
        if (!ModelState.IsValid)
        {
            await SetViewBag();
            return View(dto);
        }
        try
        {
            await _servico.Atualizar(id, dto);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Grupos, Acao = "GrupoEditado",
                Descricao = $"Grupo renomeado para '{dto.Nome}'.",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            TempData["Sucesso"] = "Grupo atualizado.";
            return RedirectToAction(nameof(Detalhes), new { slug = Slug, id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await SetViewBag();
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        try
        {
            var grupo = await _servico.ObterPorId(id);
            await _servico.Remover(id);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Grupos, Acao = "GrupoRemovido",
                Descricao = $"Grupo '{grupo?.Nome}' removido.",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            TempData["Sucesso"] = "Grupo removido.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/adicionar-membro")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarMembro(Guid id, Guid membroId)
    {
        try
        {
            await _servico.AdicionarMembro(id, membroId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Grupos, Acao = "MembroAdicionadoGrupo",
                Descricao = $"Membro adicionado ao grupo (id: {id}).",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            TempData["Sucesso"] = "Membro adicionado ao grupo.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Detalhes), new { slug = Slug, id });
    }

    [HttpPost("{id:guid}/remover-membro/{grupoMembroId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverMembro(Guid id, Guid grupoMembroId)
    {
        try
        {
            await _servico.RemoverMembro(grupoMembroId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Grupos, Acao = "MembroRemovidoGrupo",
                Descricao = $"Membro removido do grupo (grupoMembroId: {grupoMembroId}).",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress,
            });
            TempData["Sucesso"] = "Membro removido do grupo.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Detalhes), new { slug = Slug, id });
    }
}
