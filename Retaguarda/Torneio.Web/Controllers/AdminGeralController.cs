using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Common;
using Torneio.Application.DTOs.AdminGeral;
using Torneio.Application.DTOs.AdminTorneio;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Banner;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminGeral")]
[Route("admin")]
public class AdminGeralController : Controller
{
    private readonly ITorneioServico _torneioServico;
    private readonly IAdminGeralServico _adminGeralServico;
    private readonly IAdminTorneioServico _adminTorneioServico;
    private readonly IFileStorage _fileStorage;
    private readonly IBannerServico _bannerServico;

    public AdminGeralController(
        ITorneioServico torneioServico,
        IAdminGeralServico adminGeralServico,
        IAdminTorneioServico adminTorneioServico,
        IFileStorage fileStorage,
        IBannerServico bannerServico)
    {
        _torneioServico = torneioServico;
        _adminGeralServico = adminGeralServico;
        _adminTorneioServico = adminTorneioServico;
        _fileStorage = fileStorage;
        _bannerServico = bannerServico;
    }

    private async Task<string?> ResolverLogoAsync(IFormFile? arquivo, string? urlTexto)
    {
        if (arquivo != null && arquivo.Length > 0)
        {
            var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            await using var stream = arquivo.OpenReadStream();
            var path = await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "logos");
            return _fileStorage.ObterUrlPublica(path);
        }
        return string.IsNullOrWhiteSpace(urlTexto) ? null : urlTexto.Trim();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneios = await _torneioServico.ListarTodos();
        return View(torneios);
    }

    // ── Torneios ────────────────────────────────────────────────────────────

    [HttpGet("torneios/criar")]
    public IActionResult CriarTorneio()
    {
        ViewBag.ModosSorteio = Enum.GetValues<ModoSorteio>();
        ViewBag.TiposTorneio = Enum.GetValues<TipoTorneio>();
        ViewBag.PresetsJson  = BuildPresetsJson();
        var p = TorneioPresets.Get(TipoTorneio.Pesca);
        return View(new CriarTorneioDto
        {
            LabelEquipe          = p.LabelEquipe,
            LabelEquipePlural    = p.LabelEquipePlural,
            LabelMembro          = p.LabelMembro,
            LabelMembroPlural    = p.LabelMembroPlural,
            LabelSupervisor      = p.LabelSupervisor,
            LabelSupervisorPlural = p.LabelSupervisorPlural,
            LabelItem            = p.LabelItem,
            LabelItemPlural      = p.LabelItemPlural,
            LabelCaptura         = p.LabelCaptura,
            LabelCapturaPlural   = p.LabelCapturaPlural,
            MedidaCaptura        = p.MedidaCaptura,
        });
    }

    [HttpPost("torneios/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarTorneio(CriarTorneioDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ModosSorteio = Enum.GetValues<ModoSorteio>();
            ViewBag.TiposTorneio = Enum.GetValues<TipoTorneio>();
            ViewBag.PresetsJson  = BuildPresetsJson();
            return View(dto);
        }
        try
        {
            dto.LogoUrl = await ResolverLogoAsync(Request.Form.Files["logoArquivo"], dto.LogoUrl);
            await _torneioServico.Criar(dto);
            TempData["Sucesso"] = "Torneio criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.ModosSorteio = Enum.GetValues<ModoSorteio>();
            ViewBag.TiposTorneio = Enum.GetValues<TipoTorneio>();
            ViewBag.PresetsJson  = BuildPresetsJson();
            return View(dto);
        }
    }

    [HttpGet("torneios/{id:guid}/editar")]
    public async Task<IActionResult> EditarTorneio(Guid id)
    {
        var torneio = await _torneioServico.ObterPorId(id);
        if (torneio is null) return NotFound();

        ViewBag.TorneioId = id;
        ViewBag.ModosSorteio = Enum.GetValues<ModoSorteio>();
        ViewBag.TipoTorneio = torneio.TipoTorneio;
        var dto = new AtualizarTorneioDto
        {
            NomeTorneio = torneio.NomeTorneio,
            LogoUrl = torneio.LogoUrl,
            LabelEquipe = torneio.LabelEquipe,
            LabelEquipePlural = torneio.LabelEquipePlural,
            LabelMembro = torneio.LabelMembro,
            LabelMembroPlural = torneio.LabelMembroPlural,
            LabelSupervisor = torneio.LabelSupervisor,
            LabelSupervisorPlural = torneio.LabelSupervisorPlural,
            LabelItem = torneio.LabelItem,
            LabelItemPlural = torneio.LabelItemPlural,
            LabelCaptura = torneio.LabelCaptura,
            LabelCapturaPlural = torneio.LabelCapturaPlural,
            UsarFatorMultiplicador = torneio.UsarFatorMultiplicador,
            MedidaCaptura = torneio.MedidaCaptura,
            PermitirCapturaOffline = torneio.PermitirCapturaOffline,
            ModoSorteio = Enum.Parse<ModoSorteio>(torneio.ModoSorteio),
            QtdGanhadores = torneio.QtdGanhadores,
            CorPrimaria = torneio.CorPrimaria,
        };
        return View(dto);
    }

    [HttpPost("torneios/{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarTorneio(Guid id, AtualizarTorneioDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TorneioId = id;
            ViewBag.ModosSorteio = Enum.GetValues<ModoSorteio>();
            ViewBag.TipoTorneio = (await _torneioServico.ObterPorId(id))?.TipoTorneio;
            return View(dto);
        }
        try
        {
            dto.LogoUrl = await ResolverLogoAsync(Request.Form.Files["logoArquivo"], dto.LogoUrl);
            await _torneioServico.Atualizar(id, dto);
            TempData["Sucesso"] = "Torneio atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.TorneioId = id;
            ViewBag.ModosSorteio = Enum.GetValues<ModoSorteio>();
            return View(dto);
        }
    }

    [HttpPost("torneios/{id:guid}/ativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ativar(Guid id)
    {
        await _torneioServico.Ativar(id);
        TempData["Sucesso"] = "Torneio ativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("torneios/{id:guid}/desativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _torneioServico.Desativar(id);
        TempData["Sucesso"] = "Torneio desativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("torneios/{id:guid}/excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(Guid id)
    {
        try
        {
            await _torneioServico.Excluir(id);
            TempData["Sucesso"] = "Torneio excluído permanentemente.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // ── Banners ─────────────────────────────────────────────────────────────

    [HttpGet("banners")]
    public async Task<IActionResult> Banners()
    {
        var banners = await _bannerServico.ListarTodos();
        return View(banners);
    }

    [HttpGet("banners/criar")]
    public async Task<IActionResult> CriarBanner()
    {
        ViewBag.Torneios = await _torneioServico.ListarAtivos();
        return View(new CriarBannerDto());
    }

    [HttpPost("banners/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarBanner(CriarBannerDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Torneios = await _torneioServico.ListarAtivos();
            return View(dto);
        }
        try
        {
            dto.ImagemUrl = await ResolverLogoAsync(Request.Form.Files["imagemArquivo"], dto.ImagemUrl) ?? string.Empty;
            await _bannerServico.Criar(dto);
            TempData["Sucesso"] = "Banner criado com sucesso.";
            return RedirectToAction(nameof(Banners));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Torneios = await _torneioServico.ListarAtivos();
            return View(dto);
        }
    }

    [HttpPost("banners/{id:guid}/ativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtivarBanner(Guid id)
    {
        await _bannerServico.Ativar(id);
        TempData["Sucesso"] = "Banner ativado.";
        return RedirectToAction(nameof(Banners));
    }

    [HttpPost("banners/{id:guid}/desativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesativarBanner(Guid id)
    {
        await _bannerServico.Desativar(id);
        TempData["Sucesso"] = "Banner desativado.";
        return RedirectToAction(nameof(Banners));
    }

    [HttpPost("banners/{id:guid}/excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirBanner(Guid id)
    {
        await _bannerServico.Excluir(id);
        TempData["Sucesso"] = "Banner removido.";
        return RedirectToAction(nameof(Banners));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static string BuildPresetsJson() =>
        JsonSerializer.Serialize(
            TorneioPresets.Todos.ToDictionary(
                kvp => (int)kvp.Key,
                kvp => new
                {
                    kvp.Value.LabelEquipe,        kvp.Value.LabelEquipePlural,
                    kvp.Value.LabelMembro,         kvp.Value.LabelMembroPlural,
                    kvp.Value.LabelSupervisor,     kvp.Value.LabelSupervisorPlural,
                    kvp.Value.LabelItem,           kvp.Value.LabelItemPlural,
                    kvp.Value.LabelCaptura,        kvp.Value.LabelCapturaPlural,
                    kvp.Value.MedidaCaptura,
                }),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    // ── AdminsGeral ─────────────────────────────────────────────────────────

    [HttpGet("admins")]
    public async Task<IActionResult> Admins()
    {
        var admins = await _adminGeralServico.ListarTodos();
        return View(admins);
    }

    [HttpGet("admins/criar")]
    public IActionResult CriarAdmin() => View(new CriarAdminGeralDto());

    [HttpPost("admins/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarAdmin(CriarAdminGeralDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _adminGeralServico.Criar(dto);
            TempData["Sucesso"] = "Admin criado com sucesso.";
            return RedirectToAction(nameof(Admins));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("admins/{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverAdmin(Guid id)
    {
        await _adminGeralServico.Remover(id);
        TempData["Sucesso"] = "Admin removido.";
        return RedirectToAction(nameof(Admins));
    }

    // ── AdminsTorneio por torneio ────────────────────────────────────────────

    [HttpGet("torneios/{torneioId:guid}/admins")]
    public async Task<IActionResult> AdminsTorneio(Guid torneioId)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();
        ViewBag.Torneio = torneio;

        var admins = await _adminTorneioServico.ListarPorTorneio(torneioId);
        return View(admins);
    }

    [HttpGet("torneios/{torneioId:guid}/admins/criar")]
    public async Task<IActionResult> CriarAdminTorneio(Guid torneioId)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();
        ViewBag.Torneio = torneio;
        return View(new CriarAdminTorneioDto { TorneioId = torneioId });
    }

    [HttpPost("torneios/{torneioId:guid}/admins/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarAdminTorneio(Guid torneioId, CriarAdminTorneioDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();
        ViewBag.Torneio = torneio;

        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _adminTorneioServico.Criar(new CriarAdminTorneioDto
            {
                TorneioId = torneioId,
                Nome = dto.Nome,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
            });
            TempData["Sucesso"] = "Admin do torneio criado com sucesso.";
            return RedirectToAction(nameof(AdminsTorneio), new { torneioId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("torneios/{torneioId:guid}/admins/{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverAdminTorneio(Guid torneioId, Guid id)
    {
        await _adminTorneioServico.Remover(id);
        TempData["Sucesso"] = "Admin removido.";
        return RedirectToAction(nameof(AdminsTorneio), new { torneioId });
    }
}
