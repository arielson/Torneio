using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[AllowAnonymous]
[Route("{slug}/cobranca")]
public class CobrancaMembroPublicoController : TorneioBaseController
{
    private readonly ICobrancaAsaasServico _cobrancaServico;
    private readonly IMembroRepositorio _membroRepositorio;

    public CobrancaMembroPublicoController(
        TenantContext tenantContext,
        ICobrancaAsaasServico cobrancaServico,
        IMembroRepositorio membroRepositorio) : base(tenantContext)
    {
        _cobrancaServico = cobrancaServico;
        _membroRepositorio = membroRepositorio;
    }

    [HttpGet("{celular}")]
    public async Task<IActionResult> Acesso(string celular)
    {
        if (ObterMembroSessao(celular) is not null)
            return RedirectToAction(nameof(MinhasCobrancas), new { slug = Slug, celular });

        var normalizado = NormalizarCelular(celular);
        var membro = await _membroRepositorio.ObterPorCelularNormalizado(TenantContext.TorneioId, normalizado);

        ViewBag.MembroEncontrado = membro is not null;
        ViewBag.TemCpf = !string.IsNullOrWhiteSpace(membro?.Cpf) &&
                         membro!.Cpf!.Any(char.IsDigit);
        return View();
    }

    [HttpPost("{celular}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verificar(string celular, string cpf)
    {
        var normalizado = NormalizarCelular(celular);
        var membro = await _membroRepositorio.ObterPorCelularNormalizado(TenantContext.TorneioId, normalizado);

        if (membro is null)
        {
            ViewBag.Erro = "Celular não encontrado. Verifique os dados.";
            ViewBag.MembroEncontrado = false;
            ViewBag.TemCpf = false;
            return View("Acesso");
        }

        var temCpf = !string.IsNullOrWhiteSpace(membro.Cpf) && membro.Cpf.Any(char.IsDigit);

        if (temCpf)
        {
            if (!CpfCorresponde(membro.Cpf, cpf))
            {
                ViewBag.Erro = "CPF inválido. Verifique os dados e tente novamente.";
                ViewBag.MembroEncontrado = true;
                ViewBag.TemCpf = true;
                return View("Acesso");
            }
        }
        else
        {
            var digits = string.IsNullOrWhiteSpace(cpf)
                ? string.Empty
                : new string(cpf.Where(char.IsDigit).ToArray());

            if (digits.Length != 11)
            {
                ViewBag.Erro = "Informe um CPF válido com 11 dígitos para continuar.";
                ViewBag.MembroEncontrado = true;
                ViewBag.TemCpf = false;
                return View("Acesso");
            }

            membro.AtualizarCpf(cpf);
            await _membroRepositorio.Atualizar(membro);
        }

        HttpContext.Session.SetString(SessionKey(celular), membro.Id.ToString());
        return RedirectToAction(nameof(MinhasCobrancas), new { slug = Slug, celular });
    }

    [HttpGet("{celular}/cobrancas")]
    public async Task<IActionResult> MinhasCobrancas(string celular)
    {
        var membroId = ObterMembroSessao(celular);
        if (membroId is null)
            return RedirectToAction(nameof(Acesso), new { slug = Slug, celular });

        var cobrancas = await _cobrancaServico.ListarPorMembro(TenantContext.TorneioId, membroId.Value);
        return View(cobrancas);
    }

    [HttpGet("{celular}/qrcode/{parcelaId:guid}")]
    public async Task<IActionResult> QrCode(string celular, Guid parcelaId)
    {
        if (ObterMembroSessao(celular) is null)
            return Json(new { success = false, error = "Sessão expirada. Acesse novamente." });

        try
        {
            var qrCode = await _cobrancaServico.ObterQrCodePix(parcelaId);
            return Json(new { success = true, data = qrCode });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    private Guid? ObterMembroSessao(string celular)
    {
        var valor = HttpContext.Session.GetString(SessionKey(celular));
        return Guid.TryParse(valor, out var id) ? id : null;
    }

    private string SessionKey(string celular) =>
        $"cobranca_{Slug}_{NormalizarCelular(celular)}";

    private static string NormalizarCelular(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
        var digitos = new string(valor.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digitos)) return string.Empty;
        return digitos.Length is 10 or 11 ? $"+55{digitos}" : $"+{digitos}";
    }

    private static bool CpfCorresponde(string? cpfArmazenado, string? cpfDigitado)
    {
        if (string.IsNullOrWhiteSpace(cpfArmazenado) || string.IsNullOrWhiteSpace(cpfDigitado))
            return false;
        var stored = new string(cpfArmazenado.Where(char.IsDigit).ToArray());
        var typed  = new string(cpfDigitado.Where(char.IsDigit).ToArray());
        return !string.IsNullOrEmpty(stored) && stored == typed;
    }
}
