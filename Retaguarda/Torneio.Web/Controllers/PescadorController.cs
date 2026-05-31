using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Asaas;
using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Asaas;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(AuthenticationSchemes = "PescadorAuth", Policy = "Pescador")]
[Route("{slug}/pescador")]
public class PescadorController : TorneioBaseController
{
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ICobrancaAsaasServico _cobrancaServico;
    private readonly IConfiguracaoAsaasServico _configuracaoAsaasServico;
    private readonly IParcelaTorneioRepositorio _parcelaRepositorio;
    private readonly ITorneioServico _torneioServico;
    private readonly CalculadoraTaxaAsaas _calculadoraTaxa;

    public PescadorController(
        TenantContext tenantContext,
        IMembroRepositorio membroRepositorio,
        ICobrancaAsaasServico cobrancaServico,
        IConfiguracaoAsaasServico configuracaoAsaasServico,
        IParcelaTorneioRepositorio parcelaRepositorio,
        ITorneioServico torneioServico,
        CalculadoraTaxaAsaas calculadoraTaxa) : base(tenantContext)
    {
        _membroRepositorio = membroRepositorio;
        _cobrancaServico = cobrancaServico;
        _configuracaoAsaasServico = configuracaoAsaasServico;
        _parcelaRepositorio = parcelaRepositorio;
        _torneioServico = torneioServico;
        _calculadoraTaxa = calculadoraTaxa;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null) return await NaoCadastradoAsync();

        return View(membro);
    }

    // ── Lista de cobranças ────────────────────────────────────────────────────

    [HttpGet("cobrancas")]
    public async Task<IActionResult> Cobrancas()
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null) return await NaoCadastradoAsync();

        var configAsaas = await _configuracaoAsaasServico.ObterPorTorneio(TenantContext.TorneioId);
        var asaasAtivo = configAsaas?.StatusChave == "Ativa";

        var parcelas = (await _parcelaRepositorio.ListarPorMembro(membro.Id))
            .Select(p => MapearParcela(p, membro.Nome))
            .OrderBy(p => p.NumeroParcela)
            .ThenBy(p => p.Vencimento)
            .ToList();

        var cobrancastAsaas = (await _cobrancaServico.ListarPorMembro(TenantContext.TorneioId, membro.Id))
            .GroupBy(c => c.ParcelaTorneioId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CriadoEm).First());

        // Pré-calcula a taxa para cada parcela:
        // Se já tem cobrança Asaas → usa a taxa real registrada
        // Se Asaas ativo e sem cobrança → calcula taxa estimada (PIX se disponível, senão cartão)
        var taxas = new Dictionary<Guid, decimal>();
        if (asaasAtivo)
        {
            foreach (var p in parcelas)
            {
                if (cobrancastAsaas.TryGetValue(p.Id, out var c) && c.TaxaAsaas.HasValue)
                {
                    taxas[p.Id] = c.TaxaAsaas.Value;
                }
                else
                {
                    taxas[p.Id] = configAsaas!.AceitarPix
                        ? _calculadoraTaxa.CalcularTaxaPix(p.Valor)
                        : _calculadoraTaxa.CalcularTaxaCartao(p.Valor);
                }
            }
        }

        ViewBag.CobrancasAsaas = cobrancastAsaas;
        ViewBag.Taxas = taxas;
        ViewBag.AsaasAtivo = asaasAtivo;
        return View(parcelas);
    }

    // ── Detalhe / pagamento de uma cobrança ───────────────────────────────────

    [HttpGet("cobrancas/{parcelaId:guid}")]
    public async Task<IActionResult> DetalheCobranca(Guid parcelaId)
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null) return await NaoCadastradoAsync();

        var parcela = await _parcelaRepositorio.ObterPorId(parcelaId);
        if (parcela is null || parcela.MembroId != membro.Id)
            return NotFound();

        var cobranca = await _cobrancaServico.ObterPorParcelaId(parcelaId);
        var configAsaas = await _configuracaoAsaasServico.ObterPorTorneio(TenantContext.TorneioId);

        // Taxa: usa a real se já gerada, senão estima
        decimal taxa = 0;
        if (configAsaas?.StatusChave == "Ativa")
        {
            taxa = cobranca?.TaxaAsaas ?? (configAsaas.AceitarPix
                ? _calculadoraTaxa.CalcularTaxaPix(parcela.Valor)
                : _calculadoraTaxa.CalcularTaxaCartao(parcela.Valor));
        }

        ViewBag.Cobranca = cobranca;
        ViewBag.ConfigAsaas = configAsaas;
        ViewBag.CpfAtual = membro.Cpf;
        ViewBag.Taxa = taxa;
        return View(MapearParcela(parcela, membro.Nome));
    }

    [HttpPost("cobrancas/{parcelaId:guid}/gerar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GerarCobranca(Guid parcelaId, int formaPagamento, string? cpf)
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null) return await NaoCadastradoAsync();

        var parcela = await _parcelaRepositorio.ObterPorId(parcelaId);
        if (parcela is null || parcela.MembroId != membro.Id)
            return NotFound();

        // Se CPF foi informado, salva em todos os registros do pescador antes de gerar
        if (!string.IsNullOrWhiteSpace(cpf))
        {
            var todos = (await _membroRepositorio.ListarTodosPorCelular(CelularDoUsuario)).ToList();
            foreach (var m in todos)
            {
                m.AtualizarCpf(cpf);
                await _membroRepositorio.Atualizar(m);
            }
            // Recarrega membro para ter o CPF atualizado
            membro = await ResolverMembroAtualAsync();
            if (membro is null) return await NaoCadastradoAsync();
        }

        if (string.IsNullOrWhiteSpace(membro.Cpf))
        {
            TempData["Erro"] = "Informe seu CPF para continuar.";
            return RedirectToAction(nameof(DetalheCobranca), new { slug = Slug, parcelaId });
        }

        try
        {
            await _cobrancaServico.GerarCobranca(new GerarCobrancaDto
            {
                TorneioId = TenantContext.TorneioId,
                ParcelaTorneioId = parcelaId,
                FormaPagamento = (FormaPagamentoAsaas)formaPagamento
            });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(DetalheCobranca), new { slug = Slug, parcelaId });
    }

    [HttpGet("cobrancas/{parcelaId:guid}/status")]
    public async Task<IActionResult> StatusCobranca(Guid parcelaId)
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null)
            return Json(new { expirado = true });

        var parcela = await _parcelaRepositorio.ObterPorId(parcelaId);
        if (parcela is null || parcela.MembroId != membro.Id)
            return Json(new { erro = true });

        var cobranca = await _cobrancaServico.ObterPorParcelaId(parcelaId);

        return Json(new
        {
            pago = parcela.Pago,
            status = cobranca?.Status
        });
    }

    [HttpGet("cobrancas/{parcelaId:guid}/qrcode")]
    public async Task<IActionResult> QrCode(Guid parcelaId)
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null)
            return Json(new { success = false, error = "Sessão inválida." });

        var parcela = await _parcelaRepositorio.ObterPorId(parcelaId);
        if (parcela is null || parcela.MembroId != membro.Id)
            return Json(new { success = false, error = "Cobrança não encontrada." });

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

    // ── Perfil ────────────────────────────────────────────────────────────────

    [HttpGet("perfil")]
    public async Task<IActionResult> Perfil()
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null) return await NaoCadastradoAsync();

        ViewBag.TotalTorneios = (await _membroRepositorio.ListarTodosPorCelular(CelularDoUsuario)).Count();
        return View(membro);
    }

    [HttpPost("perfil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarPerfil(string? email, string? cpf)
    {
        var membro = await ResolverMembroAtualAsync();
        if (membro is null) return await NaoCadastradoAsync();

        var todos = (await _membroRepositorio.ListarTodosPorCelular(CelularDoUsuario)).ToList();
        foreach (var m in todos)
        {
            m.AtualizarEmail(email);
            m.AtualizarCpf(cpf);
            await _membroRepositorio.Atualizar(m);
        }

        TempData["Sucesso"] = $"Perfil atualizado em {todos.Count} cadastro(s).";
        return RedirectToAction(nameof(Perfil), new { slug = Slug });
    }

    // ── Não cadastrado neste torneio ──────────────────────────────────────────

    [HttpGet("nao-cadastrado")]
    public async Task<IActionResult> NaoCadastrado()
    {
        var todos = (await _membroRepositorio.ListarTodosPorCelular(CelularDoUsuario)).ToList();
        var outrosIds = todos
            .Where(m => m.TorneioId != TenantContext.TorneioId)
            .Select(m => m.TorneioId)
            .Distinct()
            .ToList();

        var outrosTorneios = new List<TorneioDto>();
        foreach (var id in outrosIds)
        {
            var t = await _torneioServico.ObterPorId(id);
            if (t is not null) outrosTorneios.Add(t);
        }

        ViewBag.OutrosTorneios = outrosTorneios;
        return View();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string CelularDoUsuario =>
        User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    private Task<Membro?> ResolverMembroAtualAsync() =>
        _membroRepositorio.ObterPorCelularNormalizado(TenantContext.TorneioId, CelularDoUsuario);

    private async Task<IActionResult> NaoCadastradoAsync()
    {
        var todos = (await _membroRepositorio.ListarTodosPorCelular(CelularDoUsuario)).ToList();
        if (todos.Any(m => m.TorneioId != TenantContext.TorneioId))
            return RedirectToAction(nameof(NaoCadastrado), new { slug = Slug });

        await HttpContext.SignOutAsync("PescadorAuth");
        return RedirectToAction("Entrar", "PescadorAuth", new { slug = Slug });
    }

    private static ParcelaTorneioDto MapearParcela(ParcelaTorneio p, string nomeMembro) =>
        new()
        {
            Id = p.Id,
            TorneioId = p.TorneioId,
            MembroId = p.MembroId,
            NomeMembro = nomeMembro,
            TipoParcela = p.TipoParcela.ToString(),
            Descricao = p.Descricao,
            NumeroParcela = p.NumeroParcela,
            Valor = p.Valor,
            Vencimento = p.Vencimento,
            VencimentoEditadoManual = p.VencimentoEditadoManual,
            Pago = p.Pago,
            DataPagamento = p.DataPagamento,
            Observacao = p.Observacao,
            Inadimplente = !p.Pago && !p.Bonificada && p.Vencimento < DateTime.UtcNow,
            Bonificada = p.Bonificada,
            MotivoBonificacao = p.MotivoBonificacao
        };
}
