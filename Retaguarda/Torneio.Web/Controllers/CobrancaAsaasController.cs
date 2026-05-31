using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Asaas;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/financeiro/asaas")]
public class CobrancaAsaasController : TorneioBaseController
{
    private readonly ICobrancaAsaasServico _cobrancaServico;
    private readonly IConfiguracaoAsaasServico _configServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public CobrancaAsaasController(
        TenantContext tenantContext,
        ICobrancaAsaasServico cobrancaServico,
        IConfiguracaoAsaasServico configServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log) : base(tenantContext)
    {
        _cobrancaServico = cobrancaServico;
        _configServico = configServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    [HttpPost("{parcelaId:guid}/gerar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gerar(Guid parcelaId, FormaPagamentoAsaas formaPagamento, string? cpf)
    {
        try
        {
            var cobranca = await _cobrancaServico.GerarCobranca(new GerarCobrancaDto
            {
                TorneioId = TenantContext.TorneioId,
                ParcelaTorneioId = parcelaId,
                FormaPagamento = formaPagamento,
                CpfOverride = cpf
            });

            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Asaas,
                Acao = "GerarCobrancaAsaas",
                Descricao = $"Cobrança Asaas gerada | Parcela: {parcelaId} | Forma: {formaPagamento} | PaymentId: {cobranca.AsaasPaymentId}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });

            TempData["Sucesso"] = $"Cobrança {formaPagamento} gerada no Asaas com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction("EditarCobranca", "Financeiro", new { slug = Slug, id = parcelaId });
    }

    [HttpGet("{parcelaId:guid}/qrcode")]
    public async Task<IActionResult> QrCode(Guid parcelaId)
    {
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

    [HttpPost("{parcelaId:guid}/cancelar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid parcelaId)
    {
        try
        {
            await _cobrancaServico.CancelarCobranca(parcelaId);

            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Asaas,
                Acao = "CancelarCobrancaAsaas",
                Descricao = $"Cobrança Asaas cancelada | Parcela: {parcelaId}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });

            TempData["Sucesso"] = "Cobrança Asaas cancelada.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction("EditarCobranca", "Financeiro", new { slug = Slug, id = parcelaId });
    }
}
