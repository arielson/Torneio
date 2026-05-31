using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Torneio.Application.Common;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[AllowAnonymous]
[Route("{slug}/pescador/entrar")]
public class PescadorAuthController : TorneioBaseController
{
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ITorneioServico _torneioServico;
    private readonly ISmsService _smsService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    private static readonly TimeSpan CodigoValidade = TimeSpan.FromHours(24);

    public PescadorAuthController(
        TenantContext tenantContext,
        IMembroRepositorio membroRepositorio,
        ITorneioServico torneioServico,
        ISmsService smsService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration) : base(tenantContext)
    {
        _membroRepositorio = membroRepositorio;
        _torneioServico = torneioServico;
        _smsService = smsService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    // ── Tela inicial ──────────────────────────────────────────────────────────

    [HttpGet("")]
    public IActionResult Entrar(string? returnUrl)
    {
        if (User.HasClaim("perfil", "Pescador"))
            return RedirectToAction("Index", "Pescador", new { slug = Slug });

        ViewBag.ReturnUrl = returnUrl;
        ViewBag.Estado = "Celular";
        ViewBag.LinkWhatsApp = LinkWhatsApp("Olá, preciso de ajuda para acessar o torneio.");
        return View();
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Identificar(string celular, string? returnUrl)
    {
        var normalizado = NormalizarCelular(celular);
        if (string.IsNullOrEmpty(normalizado))
        {
            ViewBag.Estado = "Celular";
            ViewBag.Erro = "Informe um número de celular válido.";
            return View("Entrar");
        }

        // Busca em TODOS os torneios e no torneio atual
        var todos = (await _membroRepositorio.ListarTodosPorCelular(normalizado)).ToList();
        var membroAtual = todos.FirstOrDefault(m => m.TorneioId == TenantContext.TorneioId);

        // Não cadastrado em nenhum torneio
        if (todos.Count == 0)
        {
            ViewBag.Estado = "Celular";
            ViewBag.Erro = "Número não encontrado. ";
            ViewBag.LinkWhatsApp = LinkWhatsApp("Olá, preciso de ajuda para acessar o torneio.");
            return View("Entrar");
        }

        // Cadastrado em outros torneios, mas não neste
        if (membroAtual is null)
        {
            var outrosTorneios = await ResolverOutrosTorneiosAsync(todos
                .Select(m => m.TorneioId)
                .Where(id => id != TenantContext.TorneioId)
                .Distinct()
                .ToList());

            ViewBag.Estado = "NaoNesteTorneio";
            ViewBag.OutrosTorneios = outrosTorneios;
            return View("Entrar");
        }

        // Salva celular na sessão para os próximos passos
        HttpContext.Session.SetString(CelularKey(), normalizado);
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.Celular = celular;

        // Verifica se tem senha (em qualquer torneio)
        var senhaExistente = todos.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.SenhaHash))?.SenhaHash;

        if (senhaExistente is null)
        {
            ViewBag.Estado = "PrimeiroAcesso";
            return View("Entrar");
        }

        // Se o membro deste torneio ainda não tem a senha sincronizada, corrige agora
        if (string.IsNullOrWhiteSpace(membroAtual.SenhaHash))
        {
            membroAtual.DefinirSenha(senhaExistente);
            await _membroRepositorio.Atualizar(membroAtual);
        }

        ViewBag.Estado = "Login";
        return View("Entrar");
    }

    // ── Login com senha ───────────────────────────────────────────────────────

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string senha, string? returnUrl)
    {
        var normalizado = HttpContext.Session.GetString(CelularKey());
        if (string.IsNullOrEmpty(normalizado))
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        var todos = (await _membroRepositorio.ListarTodosPorCelular(normalizado)).ToList();
        var referencia = todos.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.SenhaHash));

        if (referencia is null || !_passwordHasher.Verificar(senha, referencia.SenhaHash!))
        {
            ViewBag.Estado = "Login";
            ViewBag.Erro = "Senha incorreta.";
            ViewBag.Celular = normalizado;
            ViewBag.ReturnUrl = returnUrl;
            return View("Entrar");
        }

        await AssinarAsync(normalizado, todos.FirstOrDefault(m => m.TorneioId == TenantContext.TorneioId)?.Nome ?? referencia.Nome);
        HttpContext.Session.Remove(CelularKey());

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Pescador", new { slug = Slug });
    }

    // ── Envio de código SMS ───────────────────────────────────────────────────

    [HttpPost("enviar-codigo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCodigo(string? returnUrl)
    {
        var normalizado = HttpContext.Session.GetString(CelularKey());
        if (string.IsNullOrEmpty(normalizado))
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        var todos = (await _membroRepositorio.ListarTodosPorCelular(normalizado)).ToList();
        if (todos.Count == 0)
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        // Reutiliza código existente se ainda válido — não gera novo a cada clique
        var membroComCodigo = todos.FirstOrDefault(m => m.CodigoSmsValido());
        string codigo;
        if (membroComCodigo is not null)
        {
            codigo = membroComCodigo.CodigoSms!;
        }
        else
        {
            codigo = GerarCodigo();
            foreach (var m in todos)
            {
                m.DefinirCodigoSms(codigo, CodigoValidade);
                await _membroRepositorio.Atualizar(m);
            }
        }

        HttpContext.Session.SetString(ReturnUrlKey(), returnUrl ?? "");

        try
        {
            await _smsService.EnviarCodigoAsync(normalizado, codigo);
        }
        catch
        {
            TempData["Erro"] = $"Não foi possível enviar o SMS. Se o problema persistir, " +
                $"fale conosco pelo <a href='{LinkWhatsApp("Olá, estou com dificuldades para receber o código de acesso.")}' " +
                $"target='_blank' rel='noopener'>WhatsApp</a>.";
            TempData["ErroHtml"] = true;
            return RedirectToAction(nameof(Entrar), new { slug = Slug });
        }

        return RedirectToAction(nameof(VerificarCodigoView), new { slug = Slug });
    }

    // ── Verificação do código ─────────────────────────────────────────────────

    [HttpGet("codigo")]
    public async Task<IActionResult> VerificarCodigoView()
    {
        var normalizado = HttpContext.Session.GetString(CelularKey());
        if (string.IsNullOrEmpty(normalizado))
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        var todos = (await _membroRepositorio.ListarTodosPorCelular(normalizado)).ToList();
        if (!todos.Any(m => m.CodigoSmsValido()))
        {
            TempData["Erro"] = "Código expirado. Solicite um novo.";
            return RedirectToAction(nameof(Entrar), new { slug = Slug });
        }

        return View("VerificarCodigo");
    }

    [HttpPost("codigo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerificarCodigo(string codigo)
    {
        var normalizado = HttpContext.Session.GetString(CelularKey());
        if (string.IsNullOrEmpty(normalizado))
        {
            TempData["Erro"] = "Sessão expirada. Tente novamente.";
            return RedirectToAction(nameof(Entrar), new { slug = Slug });
        }

        var todos = (await _membroRepositorio.ListarTodosPorCelular(normalizado)).ToList();
        var comCodigo = todos.FirstOrDefault(m => m.CodigoSmsValido());
        if (comCodigo is null)
        {
            TempData["Erro"] = "Código expirado. Solicite um novo.";
            return RedirectToAction(nameof(Entrar), new { slug = Slug });
        }

        if (!string.Equals(comCodigo.CodigoSms, codigo?.Trim(), StringComparison.Ordinal))
        {
            ViewBag.Erro = "Código inválido. Verifique e tente novamente.";
            return View("VerificarCodigo");
        }

        // Limpa o código em todos os membros com este celular após uso bem-sucedido
        foreach (var m in todos)
        {
            m.LimparCodigoSms();
            await _membroRepositorio.Atualizar(m);
        }

        HttpContext.Session.SetString(CodigoOkKey(), "1");
        return RedirectToAction(nameof(DefinirSenhaView), new { slug = Slug });
    }

    // ── Definição / redefinição de senha ─────────────────────────────────────

    [HttpGet("senha")]
    public IActionResult DefinirSenhaView()
    {
        if (HttpContext.Session.GetString(CodigoOkKey()) != "1")
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        return View("DefinirSenha");
    }

    [HttpPost("senha")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DefinirSenha(string senha, string confirmar)
    {
        if (HttpContext.Session.GetString(CodigoOkKey()) != "1")
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
        {
            ViewBag.Erro = "A senha deve ter pelo menos 6 caracteres.";
            return View("DefinirSenha");
        }

        if (senha != confirmar)
        {
            ViewBag.Erro = "As senhas não coincidem.";
            return View("DefinirSenha");
        }

        var normalizado = HttpContext.Session.GetString(CelularKey());
        if (string.IsNullOrEmpty(normalizado))
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        // Propaga a nova senha para TODOS os cadastros do pescador
        var todos = (await _membroRepositorio.ListarTodosPorCelular(normalizado)).ToList();
        if (todos.Count == 0)
            return RedirectToAction(nameof(Entrar), new { slug = Slug });

        var hash = _passwordHasher.Hash(senha);
        foreach (var m in todos)
        {
            m.DefinirSenha(hash);
            await _membroRepositorio.Atualizar(m);
        }

        var returnUrl = HttpContext.Session.GetString(ReturnUrlKey());
        HttpContext.Session.Remove(CodigoOkKey());
        HttpContext.Session.Remove(CelularKey());
        HttpContext.Session.Remove(ReturnUrlKey());

        var membroAtual = todos.FirstOrDefault(m => m.TorneioId == TenantContext.TorneioId);
        await AssinarAsync(normalizado, membroAtual?.Nome ?? todos.First().Nome);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Pescador", new { slug = Slug });
    }

    // ── Sair ──────────────────────────────────────────────────────────────────

    [HttpGet("/{slug}/pescador/sair")]
    [HttpPost("/{slug}/pescador/sair")]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync("PescadorAuth");
        return Redirect($"/{Slug}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Task AssinarAsync(string celular, string nome)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, celular),
            new(ClaimTypes.Name, nome),
            new("perfil", "Pescador")
        };
        var identity = new ClaimsIdentity(claims, "PescadorAuth");
        var principal = new ClaimsPrincipal(identity);
        return HttpContext.SignInAsync("PescadorAuth", principal);
    }

    private async Task<List<TorneioDto>> ResolverOutrosTorneiosAsync(List<Guid> torneioIds)
    {
        var resultado = new List<TorneioDto>();
        foreach (var id in torneioIds)
        {
            var t = await _torneioServico.ObterPorId(id);
            if (t is not null) resultado.Add(t);
        }
        return resultado;
    }

    private string CelularKey()   => $"pesc_cel_{Slug}";
    private string CodigoOkKey()  => $"pesc_ok_{Slug}";
    private string ReturnUrlKey() => $"pesc_ret_{Slug}";

    private string LinkWhatsApp(string texto)
    {
        var numero = _configuration["Suporte:WhatsApp"] ?? "5571991844287";
        return $"https://wa.me/{numero}?text={Uri.EscapeDataString(texto)}";
    }

    private static string NormalizarCelular(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
        var digitos = new string(valor.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digitos)) return string.Empty;
        return digitos.Length is 10 or 11 ? $"+55{digitos}" : $"+{digitos}";
    }

    private static string GerarCodigo() =>
        Random.Shared.Next(100000, 999999).ToString();
}
