using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

public class HomeController : Controller
{
    private readonly IAutenticacaoServico _autenticacao;
    private readonly ITorneioServico _torneioServico;

    public HomeController(IAutenticacaoServico autenticacao, ITorneioServico torneioServico)
    {
        _autenticacao = autenticacao;
        _torneioServico = torneioServico;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var torneios = await _torneioServico.ListarAtivos();
        return View(torneios);
    }

    // AdminGeral login
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/admin");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _autenticacao.AutenticarAdminGeral(model.Usuario, model.Senha);
        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new("sub", usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new("perfil", usuario.Perfil.ToString()),
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TorneioCookie"));
        await HttpContext.SignInAsync("TorneioCookie", principal);
        return Redirect(model.ReturnUrl ?? "/admin");
    }

    // AdminTorneio login
    [HttpGet("{slug}/login")]
    public async Task<IActionResult> TorneioLogin(string slug, string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? $"/{slug}/admin");

        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null) return NotFound();

        return View(new TorneioLoginViewModel
        {
            Slug = slug,
            NomeTorneio = torneio.NomeTorneio,
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("{slug}/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TorneioLogin(string slug, TorneioLoginViewModel model)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null) return NotFound();

        model.Slug = slug;
        model.NomeTorneio = torneio.NomeTorneio;

        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _autenticacao.AutenticarAdminTorneio(model.Usuario, model.Senha, torneio.Id);
        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new("sub", usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new("perfil", usuario.Perfil.ToString()),
            new("torneio_id", torneio.Id.ToString()),
            new("slug", slug),
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TorneioCookie"));
        await HttpContext.SignInAsync("TorneioCookie", principal);
        return Redirect(model.ReturnUrl ?? $"/{slug}/admin");
    }

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("TorneioCookie");
        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet("/acesso-negado")]
    public IActionResult AcessoNegado() => View();
}
