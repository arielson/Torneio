using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Asaas;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminGeral")]
[Route("admin/asaas")]
public class AsaasAdminController : Controller
{
    private readonly IConfiguracaoAsaasServico _asaasServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public AsaasAdminController(
        IConfiguracaoAsaasServico asaasServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log)
    {
        _asaasServico = asaasServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private string AdminNome => User.Identity?.Name ?? "—";
    private string AdminPerfil => User.FindFirst("perfil")?.Value ?? "AdminGeral";
    private string? AdminIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneios = await _torneioServico.ListarTodos();
        var configs = new Dictionary<Guid, ConfiguracaoAsaasDto>();

        foreach (var t in torneios)
        {
            var config = await _asaasServico.ObterPorTorneio(t.Id);
            if (config is not null)
                configs[t.Id] = config;
        }

        ViewBag.Configs = configs;
        return View(torneios);
    }

    [HttpGet("{torneioId:guid}/configurar")]
    public async Task<IActionResult> Configurar(Guid torneioId)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();

        var configAtual = await _asaasServico.ObterPorTorneio(torneioId);

        ViewBag.Torneio = torneio;
        ViewBag.ConfigAtual = configAtual;

        var dto = new SalvarConfiguracaoAsaasDto
        {
            TorneioId = torneioId,
            AceitarPix = configAtual?.AceitarPix ?? true,
            AceitarCartaoCredito = configAtual?.AceitarCartaoCredito ?? false
        };

        return View(dto);
    }

    [HttpPost("{torneioId:guid}/configurar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarConfiguracao(Guid torneioId, SalvarConfiguracaoAsaasDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        dto.TorneioId = torneioId;

        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Torneio = torneio;
            ViewBag.ConfigAtual = await _asaasServico.ObterPorTorneio(torneioId);
            return View("Configurar", dto);
        }

        try
        {
            await _asaasServico.Salvar(dto);

            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = torneioId,
                NomeTorneio = torneio.NomeTorneio,
                Categoria = CategoriaLog.Asaas,
                Acao = "SalvarConfiguracaoAsaas",
                Descricao = $"Configuração Asaas salva | PIX: {dto.AceitarPix} | Cartão: {dto.AceitarCartaoCredito}",
                UsuarioNome = AdminNome,
                UsuarioPerfil = AdminPerfil,
                IpAddress = AdminIp
            });

            // Tenta registrar o webhook automaticamente após salvar
            try
            {
                await _asaasServico.RegistrarWebhook(torneioId);
                TempData["Sucesso"] = "Configuração Asaas salva e webhook registrado com sucesso.";
            }
            catch
            {
                TempData["Sucesso"] = "Configuração Asaas salva. Use o botão \"Registrar Webhook\" para ativar as notificações.";
            }

            return RedirectToAction(nameof(Configurar), new { torneioId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Torneio = torneio;
            ViewBag.ConfigAtual = await _asaasServico.ObterPorTorneio(torneioId);
            return View("Configurar", dto);
        }
    }

    [HttpPost("{torneioId:guid}/desativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(Guid torneioId)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();

        await _asaasServico.Desativar(torneioId);

        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio.NomeTorneio,
            Categoria = CategoriaLog.Asaas,
            Acao = "DesativarAsaas",
            Descricao = "Integração Asaas desativada",
            UsuarioNome = AdminNome,
            UsuarioPerfil = AdminPerfil,
            IpAddress = AdminIp
        });

        TempData["Sucesso"] = $"Integração Asaas desativada para {torneio.NomeTorneio}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{torneioId:guid}/reativar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(Guid torneioId)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();

        await _asaasServico.Reativar(torneioId);

        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio.NomeTorneio,
            Categoria = CategoriaLog.Asaas,
            Acao = "ReativarAsaas",
            Descricao = "Integração Asaas reativada",
            UsuarioNome = AdminNome,
            UsuarioPerfil = AdminPerfil,
            IpAddress = AdminIp
        });

        TempData["Sucesso"] = $"Integração Asaas reativada para {torneio.NomeTorneio}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{torneioId:guid}/registrar-webhook")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarWebhook(Guid torneioId)
    {
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();

        try
        {
            await _asaasServico.RegistrarWebhook(torneioId);

            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = torneioId,
                NomeTorneio = torneio.NomeTorneio,
                Categoria = CategoriaLog.Asaas,
                Acao = "RegistrarWebhookAsaas",
                Descricao = "Webhook Asaas registrado/atualizado",
                UsuarioNome = AdminNome,
                UsuarioPerfil = AdminPerfil,
                IpAddress = AdminIp
            });

            TempData["Sucesso"] = "Webhook Asaas registrado/atualizado com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Falha ao registrar webhook: {ex.Message}";
        }

        return RedirectToAction(nameof(Configurar), new { torneioId });
    }
}
