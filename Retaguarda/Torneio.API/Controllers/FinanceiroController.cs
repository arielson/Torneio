using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/financeiro")]
public class FinanceiroController : BaseController
{
    private readonly IFinanceiroTorneioServico _servico;
    private readonly IProdutoExtraTorneioServico _produtoExtraServico;
    private readonly TenantContext _tenantContext;
    private readonly IFileStorage _fileStorage;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly ITorneioServico _torneioServico;

    public FinanceiroController(
        IFinanceiroTorneioServico servico,
        IProdutoExtraTorneioServico produtoExtraServico,
        TenantContext tenantContext,
        IFileStorage fileStorage,
        ILogAuditoriaServico logAuditoriaServico,
        ITorneioServico torneioServico)
    {
        _servico = servico;
        _produtoExtraServico = produtoExtraServico;
        _tenantContext = tenantContext;
        _fileStorage = fileStorage;
        _logAuditoriaServico = logAuditoriaServico;
        _torneioServico = torneioServico;
    }

    [HttpGet("config")]
    public async Task<IActionResult> ObterConfiguracao() =>
        Ok(await _servico.ObterConfiguracao(_tenantContext.TorneioId));

    [HttpPut("config")]
    public async Task<IActionResult> AtualizarConfiguracao([FromBody] AtualizarTorneioFinanceiroDto dto)
    {
        await _servico.AtualizarConfiguracao(_tenantContext.TorneioId, dto);
        await RegistrarLog(
            "AtualizarConfiguracaoFinanceiraApp",
            $"Configuracao financeira atualizada pelo app | Valor por pescador: {dto.ValorPorMembro:0.00} | Quantidade de parcelas: {dto.QuantidadeParcelas} | Primeiro vencimento: {(dto.DataPrimeiroVencimento.HasValue ? dto.DataPrimeiroVencimento.Value.ToString("dd/MM/yyyy") : "-")} | Taxa de inscricao: {dto.TaxaInscricaoValor:0.00} | Vencimento taxa: {(dto.DataVencimentoTaxaInscricao.HasValue ? dto.DataVencimentoTaxaInscricao.Value.ToString("dd/MM/yyyy") : "-")}");
        return NoContent();
    }

    [HttpPost("cobrancas/gerar")]
    public async Task<IActionResult> GerarParcelas([FromBody] GerenciarParcelasDto dto)
    {
        if (dto.SomenteNovos)
        {
            await _servico.SincronizarParcelas(_tenantContext.TorneioId, dto.MembroIds, true);
            await RegistrarLog("GerarParcelasNovosApp", $"Parcelas geradas para pescadores novos pelo app | Quantidade filtrada: {dto.MembroIds.Count}");
        }
        else if (dto.MembroIds.Count > 0)
        {
            await _servico.SincronizarParcelas(_tenantContext.TorneioId, dto.MembroIds, false);
            await RegistrarLog("GerarParcelasSelecionadosApp", $"Parcelas regeneradas para pescadores selecionados pelo app | Quantidade: {dto.MembroIds.Count}");
        }
        else
        {
            await _servico.SincronizarParcelas(_tenantContext.TorneioId);
            await RegistrarLog("RegenerarParcelasApp", "Parcelas regeneradas manualmente pelo app.");
        }
        return NoContent();
    }

    [HttpGet("indicadores")]
    public async Task<IActionResult> Indicadores() =>
        Ok(await _servico.ObterIndicadores(_tenantContext.TorneioId));

    [HttpGet("relatorios")]
    public async Task<IActionResult> Relatorios() =>
        Ok(await _servico.ObterRelatorio(_tenantContext.TorneioId));

    [HttpGet("cobrancas")]
    public async Task<IActionResult> Cobrancas(
        [FromQuery] Guid? membroId,
        [FromQuery] bool inadimplentes = false,
        [FromQuery] bool naoPagas = false,
        [FromQuery] string? tipo = null) =>
        Ok(await _servico.ListarParcelas(_tenantContext.TorneioId, membroId, inadimplentes, naoPagas, tipo));

    [HttpGet("extras")]
    public async Task<IActionResult> ProdutosExtras() =>
        Ok(await _produtoExtraServico.ListarProdutos(_tenantContext.TorneioId));

    [HttpGet("extras/{id:guid}/membros")]
    public async Task<IActionResult> ProdutoExtraMembros(Guid id) =>
        Ok(await _produtoExtraServico.ListarAderidos(id));

    [HttpPost("extras")]
    public async Task<IActionResult> CriarProdutoExtra([FromBody] CriarProdutoExtraTorneioDto dto)
    {
        var criado = await _produtoExtraServico.CriarProduto(new CriarProdutoExtraTorneioDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Valor = dto.Valor,
            Descricao = dto.Descricao
        });
        await RegistrarLog("CriarProdutoExtraApp", $"Produto extra criado pelo app | Nome: {criado.Nome} | Valor: {criado.Valor:0.00}");
        return CreatedAtAction(nameof(ProdutosExtras), new { slug = RouteData.Values["slug"] }, criado);
    }

    [HttpPut("extras/{id:guid}")]
    public async Task<IActionResult> AtualizarProdutoExtra(Guid id, [FromBody] AtualizarProdutoExtraTorneioDto dto)
    {
        await _produtoExtraServico.AtualizarProduto(id, dto);
        await RegistrarLog("AtualizarProdutoExtraApp", $"Produto extra atualizado pelo app | Produto: {id} | Nome: {dto.Nome} | Valor: {dto.Valor:0.00} | Ativo: {dto.Ativo}");
        return NoContent();
    }

    [HttpDelete("extras/{id:guid}")]
    public async Task<IActionResult> RemoverProdutoExtra(Guid id)
    {
        await _produtoExtraServico.RemoverProduto(id);
        await RegistrarLog("RemoverProdutoExtraApp", $"Produto extra removido pelo app | Produto: {id}");
        return NoContent();
    }

    [HttpPost("extras/{id:guid}/membros")]
    public async Task<IActionResult> AdicionarMembroProdutoExtra(Guid id, [FromBody] CriarProdutoExtraMembroDto dto)
    {
        await _produtoExtraServico.AdicionarMembro(new CriarProdutoExtraMembroDto
        {
            TorneioId = _tenantContext.TorneioId,
            ProdutoExtraTorneioId = id,
            MembroId = dto.MembroId,
            Quantidade = dto.Quantidade,
            ValorCobrado = dto.ValorCobrado,
            Observacao = dto.Observacao
        });
        await RegistrarLog("AdicionarMembroProdutoExtraApp", $"Venda de produto extra registrada pelo app | Produto: {id} | Membro: {dto.MembroId} | Quantidade: {dto.Quantidade:0.##} | Valor total: {dto.ValorCobrado:0.00}");
        return NoContent();
    }

    [HttpDelete("extras/membros/{produtoExtraMembroId:guid}")]
    public async Task<IActionResult> RemoverMembroProdutoExtra(Guid produtoExtraMembroId)
    {
        await _produtoExtraServico.RemoverMembro(produtoExtraMembroId);
        await RegistrarLog("RemoverMembroProdutoExtraApp", $"Venda de produto extra removida pelo app | Adesao: {produtoExtraMembroId}");
        return NoContent();
    }

    [HttpGet("cobrancas/inadimplencia")]
    public async Task<IActionResult> Inadimplencia() =>
        Ok(await _servico.ListarParcelas(_tenantContext.TorneioId, null, true, false, null));

    [HttpPut("cobrancas/{id:guid}")]
    public async Task<IActionResult> AtualizarCobranca(Guid id, [FromBody] AtualizarParcelaTorneioDto dto)
    {
        await _servico.AtualizarParcela(id, dto);
        await RegistrarLog(
            "AtualizarParcelaApp",
            $"Parcela atualizada pelo app | Parcela: {id} | Vencimento: {dto.Vencimento:dd/MM/yyyy} | Observacao: {dto.Observacao ?? "-"}");
        return NoContent();
    }

    [HttpPut("cobrancas/{id:guid}/pagamento")]
    public async Task<IActionResult> AtualizarPagamentoCobranca(Guid id, [FromBody] AtualizarPagamentoParcelaDto dto)
    {
        await _servico.AtualizarPagamento(id, dto);
        await RegistrarLog(
            dto.Pago ? "MarcarParcelaPagaApp" : "DesmarcarPagamentoParcelaApp",
            $"Pagamento de parcela atualizado pelo app | Parcela: {id} | Pago: {dto.Pago} | Data pagamento: {(dto.DataPagamento.HasValue ? dto.DataPagamento.Value.ToString("dd/MM/yyyy") : "-")}");
        return NoContent();
    }

    [HttpPost("cobrancas/{id:guid}/comprovante")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadComprovanteCobranca(Guid id, IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest(new { erro = "Informe o comprovante." });

        var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        await using var stream = arquivo.OpenReadStream();
        var url = await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "documentos/comprovantes");
        await _servico.AtualizarComprovante(id, arquivo.FileName, url, arquivo.ContentType, User.Identity?.Name ?? "App");
        await RegistrarLog(
            "UploadComprovanteParcelaApp",
            $"Comprovante anexado pelo app | Parcela: {id} | Arquivo: {arquivo.FileName}");
        return Ok(new { url });
    }

    private async Task RegistrarLog(string acao, string descricao)
    {
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = _tenantContext.TorneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Financeiro,
            Acao = acao,
            Descricao = descricao,
            UsuarioNome = User.Identity?.Name ?? "-",
            UsuarioPerfil = GetPerfil(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }
}
