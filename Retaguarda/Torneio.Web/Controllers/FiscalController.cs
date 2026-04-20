using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/fiscais")]
public class FiscalController : TorneioBaseController
{
    private readonly IFiscalServico _servico;
    private readonly IEquipeServico _equipeServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public FiscalController(TenantContext tenantContext, IFiscalServico servico, IEquipeServico equipeServico, ITorneioServico torneioServico, ILogAuditoriaServico log)
        : base(tenantContext)
    {
        _servico = servico;
        _equipeServico = equipeServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private async Task SetTorneioViewBag()
        => ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);

    private async Task SetEquipesViewBag()
        => ViewBag.Equipes = await _equipeServico.ListarTodos();

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetTorneioViewBag();
        await SetEquipesViewBag();
        var fiscais = await _servico.ListarTodos();
        return View(fiscais);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetTorneioViewBag();
        await SetEquipesViewBag();
        return View(new CriarFiscalDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarFiscalDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));

        if (!ModelState.IsValid)
        {
            await SetTorneioViewBag();
            await SetEquipesViewBag();
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
                EquipeIds = dto.EquipeIds,
            });
            TempData["Sucesso"] = "Fiscal criado com sucesso.";
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Usuarios, Acao = "CriarFiscal",
                Descricao = $"Fiscal criado: {dto.Nome} ({dto.Usuario})",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            await SetTorneioViewBag();
            await SetEquipesViewBag();
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var fiscal = await _servico.ObterPorId(id);
        if (fiscal is null) return NotFound();

        ViewBag.Fiscal = fiscal;
        await SetTorneioViewBag();
        await SetEquipesViewBag();

        return View(new AtualizarFiscalDto
        {
            Nome = fiscal.Nome,
            Usuario = fiscal.Usuario,
            FotoUrl = fiscal.FotoUrl,
            EquipeIds = fiscal.EquipeIds
        });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarFiscalDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Fiscal = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            await SetEquipesViewBag();
            return View(dto);
        }

        try
        {
            var fiscalAtual = await _servico.ObterPorId(id);
            if (fiscalAtual is null) return NotFound();

            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/fiscais") ?? fiscalAtual.FotoUrl;

            dto = new AtualizarFiscalDto
            {
                Nome = dto.Nome,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
                FotoUrl = fotoUrl,
                EquipeIds = dto.EquipeIds
            };

            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Fiscal atualizado.";

            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Usuarios,
                Acao = "EditarFiscal",
                Descricao = $"Fiscal editado: {dto.Nome} ({dto.Usuario})",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });

            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ViewBag.Fiscal = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            await SetEquipesViewBag();
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        var fiscal = await _servico.ObterPorId(id);
        await _servico.Remover(id);
        TempData["Sucesso"] = "Fiscal removido.";
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Usuarios, Acao = "RemoverFiscal",
            Descricao = $"Fiscal removido: {fiscal?.Nome ?? id.ToString()}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
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
