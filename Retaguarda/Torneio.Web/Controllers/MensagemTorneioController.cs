using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Route("{slug}/mensagens")]
[Authorize(Policy = "AdminTorneio")]
public class MensagemTorneioController : TorneioBaseController
{
    private readonly IMensagemTorneioServico _servico;

    public MensagemTorneioController(TenantContext tenantContext, IMensagemTorneioServico servico)
        : base(tenantContext)
    {
        _servico = servico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var mensagens = await _servico.ListarAsync();
        return View(mensagens);
    }

    [HttpGet("nova")]
    public IActionResult Nova() => View();

    [HttpPost("nova")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nova([FromForm] string Titulo, [FromForm] string Corpo)
    {
        if (string.IsNullOrWhiteSpace(Titulo) || string.IsNullOrWhiteSpace(Corpo))
        {
            ModelState.AddModelError("", "Título e corpo são obrigatórios.");
            return View();
        }

        await _servico.EnviarAsync(Titulo.Trim(), Corpo.Trim(), UsuarioNome);
        TempData["Sucesso"] = "Mensagem enviada para os seguidores do torneio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.RemoverAsync(id);
        TempData["Sucesso"] = "Mensagem removida.";
        return RedirectToAction(nameof(Index));
    }
}
