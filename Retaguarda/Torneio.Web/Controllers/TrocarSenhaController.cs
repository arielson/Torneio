using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

[Authorize]
[Route("trocar-senha")]
public class TrocarSenhaController : Controller
{
    private readonly IAutenticacaoServico _autenticacao;

    public TrocarSenhaController(IAutenticacaoServico autenticacao)
    {
        _autenticacao = autenticacao;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        // Se não precisa mais trocar, redireciona para home
        if (!User.HasClaim("deve_alterar_senha", "true"))
            return Redirect(UrlInicio());

        return View(new TrocarSenhaViewModel());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TrocarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.NovaSenha != model.ConfirmarSenha)
        {
            ModelState.AddModelError(nameof(model.ConfirmarSenha), "As senhas não coincidem.");
            return View(model);
        }

        var usuarioId = Guid.Parse(User.FindFirstValue("sub")!);
        var perfil    = User.FindFirstValue("perfil")!;
        var torneioIdStr = User.FindFirstValue("torneio_id");
        var torneioId = torneioIdStr is not null ? Guid.Parse(torneioIdStr) : (Guid?)null;

        try
        {
            await _autenticacao.TrocarSenha(usuarioId, perfil, model.SenhaAtual, model.NovaSenha, torneioId);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        // Re-emite o cookie sem o claim deve_alterar_senha
        var claimsAtuais = User.Claims
            .Where(c => c.Type != "deve_alterar_senha")
            .ToList();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claimsAtuais, "TorneioCookie"));
        await HttpContext.SignInAsync("TorneioCookie", principal);

        TempData["Sucesso"] = "Senha alterada com sucesso!";
        return Redirect(UrlInicio());
    }

    private string UrlInicio()
    {
        var perfil = User.FindFirstValue("perfil") ?? "";
        var slug   = User.FindFirstValue("slug");

        return perfil switch
        {
            "AdminGeral"   => "/admin",
            "AdminTorneio" => slug is not null ? $"/{slug}/admin"  : "/",
            "Fiscal"       => slug is not null ? $"/{slug}/capturas" : "/",
            "Membro"       => slug is not null ? $"/{slug}/minhas-cobrancas" : "/",
            _              => "/"
        };
    }
}
