using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/financeiro")]
public class FinanceiroController : TorneioBaseController
{
    private readonly IFinanceiroTorneioServico _financeiroServico;
    private readonly IProdutoExtraTorneioServico _produtoExtraServico;
    private readonly IDoacaoPatrocinadorServico _doacaoPatrocinadorServico;
    private readonly ICustoTorneioServico _custoServico;
    private readonly IChecklistTorneioItemServico _checklistServico;
    private readonly IAdminTorneioServico _adminTorneioServico;
    private readonly IMembroServico _membroServico;
    private readonly IPatrocinadorServico _patrocinadorServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;
    private readonly IFileStorage _fileStorage;

    public FinanceiroController(
        TenantContext tenantContext,
        IFinanceiroTorneioServico financeiroServico,
        IProdutoExtraTorneioServico produtoExtraServico,
        IDoacaoPatrocinadorServico doacaoPatrocinadorServico,
        ICustoTorneioServico custoServico,
        IChecklistTorneioItemServico checklistServico,
        IAdminTorneioServico adminTorneioServico,
        IMembroServico membroServico,
        IPatrocinadorServico patrocinadorServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log,
        IFileStorage fileStorage) : base(tenantContext)
    {
        _financeiroServico = financeiroServico;
        _produtoExtraServico = produtoExtraServico;
        _doacaoPatrocinadorServico = doacaoPatrocinadorServico;
        _custoServico = custoServico;
        _checklistServico = checklistServico;
        _adminTorneioServico = adminTorneioServico;
        _membroServico = membroServico;
        _patrocinadorServico = patrocinadorServico;
        _torneioServico = torneioServico;
        _log = log;
        _fileStorage = fileStorage;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var vm = new FinanceiroDashboardViewModel
        {
            Indicadores = await _financeiroServico.ObterIndicadores(TenantContext.TorneioId)
        };
        return View(vm);
    }

    [HttpGet("configuracao")]
    public async Task<IActionResult> Configuracao()
    {
        return View(await _financeiroServico.ObterConfiguracao(TenantContext.TorneioId));
    }

    [HttpPost("configuracao")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarConfiguracao(AtualizarTorneioFinanceiroDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Configuracao), new TorneioFinanceiroConfigDto
            {
                TorneioId = TenantContext.TorneioId,
                ValorPorMembro = dto.ValorPorMembro,
                QuantidadeParcelas = dto.QuantidadeParcelas,
                DataPrimeiroVencimento = dto.DataPrimeiroVencimento,
                TaxaInscricaoValor = dto.TaxaInscricaoValor,
                DataVencimentoTaxaInscricao = dto.DataVencimentoTaxaInscricao,
                PossuiConfiguracaoAnterior = true
            });
        }

        try
        {
            await _financeiroServico.AtualizarConfiguracao(TenantContext.TorneioId, dto);
            TempData["Sucesso"] = "Configuracao financeira atualizada.";
            await RegistrarLog(
                "AtualizarConfiguracaoFinanceiraWeb",
                $"Configuracao financeira atualizada pela retaguarda web | Valor por pescador: {dto.ValorPorMembro:0.00} | Quantidade de parcelas: {dto.QuantidadeParcelas} | Primeiro vencimento: {(dto.DataPrimeiroVencimento.HasValue ? dto.DataPrimeiroVencimento.Value.ToString("dd/MM/yyyy") : "-")} | Taxa de inscricao: {dto.TaxaInscricaoValor:0.00} | Vencimento taxa: {(dto.DataVencimentoTaxaInscricao.HasValue ? dto.DataVencimentoTaxaInscricao.Value.ToString("dd/MM/yyyy") : "-")}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Configuracao), new { slug = Slug });
    }

    [HttpPost("cobrancas/gerar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GerarParcelas(GerenciarParcelasDto dto)
    {
        try
        {
            if (dto.SomenteNovos)
            {
                await _financeiroServico.SincronizarParcelas(TenantContext.TorneioId, dto.MembroIds, true);
                TempData["Sucesso"] = "Parcelas geradas para pescadores novos.";
                await RegistrarLog("GerarParcelasNovosWeb", $"Parcelas geradas para pescadores novos pela retaguarda web | Quantidade filtrada: {dto.MembroIds.Count}");
            }
            else if (dto.MembroIds.Count > 0)
            {
                await _financeiroServico.SincronizarParcelas(TenantContext.TorneioId, dto.MembroIds, false);
                TempData["Sucesso"] = "Parcelas regeneradas para os pescadores selecionados.";
                await RegistrarLog("GerarParcelasSelecionadosWeb", $"Parcelas regeneradas para pescadores selecionados pela retaguarda web | Quantidade: {dto.MembroIds.Count}");
            }
            else
            {
                await _financeiroServico.SincronizarParcelas(TenantContext.TorneioId);
                TempData["Sucesso"] = "Parcelas regeneradas com sucesso.";
                await RegistrarLog("RegenerarParcelasWeb", "Parcelas regeneradas manualmente pela retaguarda web.");
            }
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Cobrancas), new { slug = Slug });
    }

    [HttpGet("cobrancas")]
    public async Task<IActionResult> Cobrancas(Guid? membroId, bool inadimplentes = false, bool naoPagas = false, string? tipo = null)
    {
        ViewBag.Membros = await _membroServico.ListarTodos();
        ViewBag.MembroId = membroId;
        ViewBag.Inadimplentes = inadimplentes;
        ViewBag.NaoPagas = naoPagas;
        ViewBag.Tipo = tipo;
        var parcelas = await _financeiroServico.ListarParcelas(TenantContext.TorneioId, membroId, inadimplentes, naoPagas, tipo);
        return View("Parcelas", parcelas);
    }

    [HttpGet("cobrancas/{id:guid}/editar")]
    public async Task<IActionResult> EditarCobranca(Guid id)
    {
        var parcela = await _financeiroServico.ObterParcela(id);
        return parcela is null ? NotFound() : View("EditarParcela", parcela);
    }

    [HttpPost("cobrancas/{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCobranca(Guid id, AtualizarParcelaTorneioDto dto)
    {
        try
        {
            await _financeiroServico.AtualizarParcela(id, dto);
            TempData["Sucesso"] = "Cobranca atualizada.";
            await RegistrarLog(
                "AtualizarParcelaWeb",
                $"Parcela atualizada pela retaguarda web | Parcela: {id} | Vencimento: {dto.Vencimento:dd/MM/yyyy} | Observacao: {dto.Observacao ?? "-"}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(EditarCobranca), new { slug = Slug, id });
    }

    [HttpPost("cobrancas/{id:guid}/pagamento")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagamentoCobranca(Guid id, AtualizarPagamentoParcelaDto dto)
    {
        try
        {
            await _financeiroServico.AtualizarPagamento(id, dto);
            TempData["Sucesso"] = dto.Pago ? "Cobranca marcada como paga." : "Pagamento removido da cobranca.";
            await RegistrarLog(
                dto.Pago ? "MarcarParcelaPagaWeb" : "DesmarcarPagamentoParcelaWeb",
                $"Pagamento de parcela atualizado pela retaguarda web | Parcela: {id} | Pago: {dto.Pago} | Data pagamento: {(dto.DataPagamento.HasValue ? dto.DataPagamento.Value.ToString("dd/MM/yyyy") : "-")}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(EditarCobranca), new { slug = Slug, id });
    }

    [HttpPost("cobrancas/{id:guid}/comprovante")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadComprovanteCobranca(Guid id, IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            TempData["Erro"] = "Informe o comprovante.";
            return RedirectToAction(nameof(EditarCobranca), new { slug = Slug, id });
        }

        try
        {
            await using var stream = arquivo.OpenReadStream();
            var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            var url = await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "documentos/comprovantes");
            await _financeiroServico.AtualizarComprovante(id, arquivo.FileName, url, arquivo.ContentType, UsuarioNome);
            TempData["Sucesso"] = "Comprovante anexado com sucesso.";
            await RegistrarLog(
                "UploadComprovanteParcelaWeb",
                $"Comprovante anexado pela retaguarda web | Parcela: {id} | Arquivo: {arquivo.FileName}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(EditarCobranca), new { slug = Slug, id });
    }

    [HttpGet("custos")]
    public async Task<IActionResult> Custos()
    {
        var custos = await _custoServico.Listar(TenantContext.TorneioId);
        return View(custos);
    }

    [HttpGet("custos/criar")]
    public IActionResult CriarCusto() => View(new CriarCustoTorneioDto { TorneioId = TenantContext.TorneioId });

    [HttpPost("custos/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarCusto(CriarCustoTorneioDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var criado = await _custoServico.Criar(new CriarCustoTorneioDto
            {
                TorneioId = TenantContext.TorneioId,
                Categoria = dto.Categoria,
                Descricao = dto.Descricao,
                Quantidade = dto.Quantidade,
                ValorUnitario = dto.ValorUnitario,
                Responsavel = dto.Responsavel,
                Observacao = dto.Observacao
            });
            TempData["Sucesso"] = "Custo criado com sucesso.";
            await RegistrarLog(
                "CriarCustoWeb",
                $"Custo criado pela retaguarda web | Categoria: {criado.Categoria} | Descricao: {criado.Descricao} | Valor total: {criado.ValorTotal:0.00}");
            return RedirectToAction(nameof(Custos), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet("custos/{id:guid}/editar")]
    public async Task<IActionResult> EditarCusto(Guid id)
    {
        var custo = await _custoServico.ObterPorId(id);
        if (custo is null) return NotFound();

        return View(new AtualizarCustoTorneioDto
        {
            Categoria = Enum.TryParse<CategoriaCustoTorneio>(custo.Categoria, out var categoria)
                ? categoria
                : CategoriaCustoTorneio.Outros,
            Descricao = custo.Descricao,
            Quantidade = custo.Quantidade,
            ValorUnitario = custo.ValorUnitario,
            Responsavel = custo.Responsavel,
            Observacao = custo.Observacao
        });
    }

    [HttpPost("custos/{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCusto(Guid id, AtualizarCustoTorneioDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _custoServico.Atualizar(id, dto);
            TempData["Sucesso"] = "Custo atualizado.";
            await RegistrarLog(
                "AtualizarCustoWeb",
                $"Custo atualizado pela retaguarda web | Custo: {id} | Categoria: {dto.Categoria} | Descricao: {dto.Descricao} | Quantidade: {dto.Quantidade:0.##} | Valor unitario: {dto.ValorUnitario:0.00}");
            return RedirectToAction(nameof(Custos), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("custos/{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverCusto(Guid id)
    {
        try
        {
            await _custoServico.Remover(id);
            TempData["Sucesso"] = "Custo removido.";
            await RegistrarLog("RemoverCustoWeb", $"Custo removido pela retaguarda web | Custo: {id}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Custos), new { slug = Slug });
    }

    [HttpGet("extras")]
    public async Task<IActionResult> Extras()
    {
        var produtos = await _produtoExtraServico.ListarProdutos(TenantContext.TorneioId);
        return View(produtos);
    }

    [HttpGet("extras/vendas")]
    public async Task<IActionResult> VendaExtra(Guid? produtoId)
    {
        var produtos = (await _produtoExtraServico.ListarProdutos(TenantContext.TorneioId)).OrderBy(x => x.Nome).ToList();
        ViewBag.Produtos = produtos;
        ViewBag.ProdutoIdSelecionado = produtoId;
        ViewBag.ProdutoSelecionado = produtoId.HasValue
            ? produtos.FirstOrDefault(x => x.Id == produtoId.Value)
            : null;
        ViewBag.Membros = await _membroServico.ListarTodos();

        var modelo = new CriarProdutoExtraMembroDto
        {
            TorneioId = TenantContext.TorneioId,
            ProdutoExtraTorneioId = produtoId ?? Guid.Empty,
            Quantidade = 1,
            ValorCobrado = produtos.FirstOrDefault(x => x.Id == produtoId)?.Valor ?? 0
        };

        ViewBag.Vendas = produtoId.HasValue
            ? await _produtoExtraServico.ListarAderidos(produtoId.Value)
            : Enumerable.Empty<ProdutoExtraMembroDto>();

        return View(modelo);
    }

    [HttpPost("extras/vendas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VendaExtra(CriarProdutoExtraMembroDto dto)
    {
        try
        {
            await _produtoExtraServico.AdicionarMembro(new CriarProdutoExtraMembroDto
            {
                TorneioId = TenantContext.TorneioId,
                ProdutoExtraTorneioId = dto.ProdutoExtraTorneioId,
                MembroId = dto.MembroId,
                Quantidade = dto.Quantidade,
                ValorCobrado = dto.ValorCobrado,
                Observacao = dto.Observacao
            });
            TempData["Sucesso"] = "Venda registrada com sucesso.";
            await RegistrarLog("AdicionarMembroProdutoExtraWeb", $"Venda de produto extra criada pela retaguarda web | Produto: {dto.ProdutoExtraTorneioId} | Membro: {dto.MembroId} | Quantidade: {dto.Quantidade:0.##} | Valor total: {dto.ValorCobrado:0.00}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(VendaExtra), new { slug = Slug, produtoId = dto.ProdutoExtraTorneioId });
    }

    [HttpGet("extras/criar")]
    public IActionResult CriarExtra() => View(new CriarProdutoExtraTorneioDto { TorneioId = TenantContext.TorneioId });

    [HttpPost("extras/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarExtra(CriarProdutoExtraTorneioDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _produtoExtraServico.CriarProduto(new CriarProdutoExtraTorneioDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Valor = dto.Valor,
                Descricao = dto.Descricao
            });
            TempData["Sucesso"] = "Produto extra criado com sucesso.";
            await RegistrarLog("CriarProdutoExtraWeb", $"Produto extra criado pela retaguarda web | Nome: {dto.Nome} | Valor: {dto.Valor:0.00}");
            return RedirectToAction(nameof(Extras), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet("extras/{id:guid}/editar")]
    public async Task<IActionResult> EditarExtra(Guid id)
    {
        var produto = await _produtoExtraServico.ObterProduto(id);
        if (produto is null) return NotFound();

        return View(new AtualizarProdutoExtraTorneioDto
        {
            Nome = produto.Nome,
            Valor = produto.Valor,
            Descricao = produto.Descricao,
            Ativo = produto.Ativo
        });
    }

    [HttpPost("extras/{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarExtra(Guid id, AtualizarProdutoExtraTorneioDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _produtoExtraServico.AtualizarProduto(id, dto);
            TempData["Sucesso"] = "Produto extra atualizado.";
            await RegistrarLog("AtualizarProdutoExtraWeb", $"Produto extra atualizado pela retaguarda web | Produto: {id} | Nome: {dto.Nome} | Valor: {dto.Valor:0.00} | Ativo: {dto.Ativo}");
            return RedirectToAction(nameof(Extras), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost("extras/{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverExtra(Guid id)
    {
        try
        {
            await _produtoExtraServico.RemoverProduto(id);
            TempData["Sucesso"] = "Produto extra removido.";
            await RegistrarLog("RemoverProdutoExtraWeb", $"Produto extra removido pela retaguarda web | Produto: {id}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Extras), new { slug = Slug });
    }

    [HttpGet("extras/{id:guid}/membros")]
    public async Task<IActionResult> ExtraMembros(Guid id)
    {
        var produto = await _produtoExtraServico.ObterProduto(id);
        if (produto is null) return NotFound();

        ViewBag.Produto = produto;
        ViewBag.Membros = await _membroServico.ListarTodos();
        return View(new Tuple<IEnumerable<ProdutoExtraMembroDto>, CriarProdutoExtraMembroDto>(
            await _produtoExtraServico.ListarAderidos(id),
            new CriarProdutoExtraMembroDto
            {
                TorneioId = TenantContext.TorneioId,
                ProdutoExtraTorneioId = id,
                Quantidade = 1,
                ValorCobrado = produto.Valor
            }));
    }

    [HttpPost("extras/{id:guid}/membros")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExtraMembros(Guid id, CriarProdutoExtraMembroDto dto)
    {
        try
        {
            await _produtoExtraServico.AdicionarMembro(new CriarProdutoExtraMembroDto
            {
                TorneioId = TenantContext.TorneioId,
                ProdutoExtraTorneioId = id,
                MembroId = dto.MembroId,
                Quantidade = dto.Quantidade,
                ValorCobrado = dto.ValorCobrado,
                Observacao = dto.Observacao
            });
            TempData["Sucesso"] = "Venda registrada com sucesso.";
            await RegistrarLog("AdicionarMembroProdutoExtraWeb", $"Venda de produto extra criada pela retaguarda web | Produto: {id} | Membro: {dto.MembroId} | Quantidade: {dto.Quantidade:0.##} | Valor total: {dto.ValorCobrado:0.00}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(ExtraMembros), new { slug = Slug, id });
    }

    [HttpPost("extras/membros/{produtoExtraMembroId:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverExtraMembro(Guid produtoExtraMembroId, Guid produtoId)
    {
        try
        {
            await _produtoExtraServico.RemoverMembro(produtoExtraMembroId);
            TempData["Sucesso"] = "Venda removida.";
            await RegistrarLog("RemoverMembroProdutoExtraWeb", $"Venda de produto extra removida pela retaguarda web | Adesao: {produtoExtraMembroId}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(ExtraMembros), new { slug = Slug, id = produtoId });
    }

    [HttpGet("doacoes")]
    public async Task<IActionResult> Doacoes()
    {
        var doacoes = await _doacaoPatrocinadorServico.Listar(TenantContext.TorneioId);
        return View(doacoes);
    }

    [HttpGet("doacoes/criar")]
    public async Task<IActionResult> CriarDoacao()
    {
        ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(TenantContext.TorneioId);
        return View(new CriarDoacaoPatrocinadorDto { TorneioId = TenantContext.TorneioId, DataDoacao = DateTime.UtcNow.Date });
    }

    [HttpPost("doacoes/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarDoacao(CriarDoacaoPatrocinadorDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        if (!ModelState.IsValid)
        {
            ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }

        try
        {
            var criada = await _doacaoPatrocinadorServico.Criar(new CriarDoacaoPatrocinadorDto
            {
                TorneioId = TenantContext.TorneioId,
                PatrocinadorId = dto.PatrocinadorId,
                NomePatrocinador = string.Empty,
                Tipo = dto.Tipo,
                Descricao = dto.Descricao,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor,
                Observacao = dto.Observacao,
                DataDoacao = dto.DataDoacao
            });
            TempData["Sucesso"] = "Doacao registrada com sucesso.";
            await RegistrarLog(
                "CriarDoacaoPatrocinadorWeb",
                $"Doacao registrada pela retaguarda web | Patrocinador: {criada.NomePatrocinador} | Tipo: {criada.Tipo} | Descricao: {criada.Descricao} | Valor: {(criada.Valor ?? 0):0.00}");
            return RedirectToAction(nameof(Doacoes), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }
    }

    [HttpGet("doacoes/{id:guid}/editar")]
    public async Task<IActionResult> EditarDoacao(Guid id)
    {
        var doacao = await _doacaoPatrocinadorServico.ObterPorId(id);
        if (doacao is null) return NotFound();

        ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(TenantContext.TorneioId);
        return View(new AtualizarDoacaoPatrocinadorDto
        {
            PatrocinadorId = doacao.PatrocinadorId,
            NomePatrocinador = string.Empty,
            Tipo = Enum.TryParse<Torneio.Domain.Enums.TipoDoacaoPatrocinador>(doacao.Tipo, out var tipo)
                ? tipo
                : Torneio.Domain.Enums.TipoDoacaoPatrocinador.Dinheiro,
            Descricao = doacao.Descricao,
            Quantidade = doacao.Quantidade,
            Valor = doacao.Valor,
            Observacao = doacao.Observacao,
            DataDoacao = doacao.DataDoacao
        });
    }

    [HttpPost("doacoes/{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarDoacao(Guid id, AtualizarDoacaoPatrocinadorDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }

        try
        {
            await _doacaoPatrocinadorServico.Atualizar(id, dto);
            TempData["Sucesso"] = "Doacao atualizada.";
            await RegistrarLog(
                "AtualizarDoacaoPatrocinadorWeb",
                $"Doacao atualizada pela retaguarda web | Doacao: {id} | Tipo: {dto.Tipo} | Descricao: {dto.Descricao} | Valor: {(dto.Valor ?? 0):0.00}");
            return RedirectToAction(nameof(Doacoes), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }
    }

    [HttpPost("doacoes/{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverDoacao(Guid id)
    {
        try
        {
            await _doacaoPatrocinadorServico.Remover(id);
            TempData["Sucesso"] = "Doacao removida.";
            await RegistrarLog("RemoverDoacaoPatrocinadorWeb", $"Doacao removida pela retaguarda web | Doacao: {id}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Doacoes), new { slug = Slug });
    }

    [HttpGet("checklist")]
    public async Task<IActionResult> Checklist()
    {
        var itens = await _checklistServico.Listar(TenantContext.TorneioId);
        return View(itens);
    }

    [HttpGet("checklist/criar")]
    public async Task<IActionResult> CriarChecklist()
    {
        ViewBag.AdminsTorneio = await _adminTorneioServico.ListarPorTorneio(TenantContext.TorneioId);
        return View(new CriarChecklistTorneioItemDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("checklist/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarChecklist(CriarChecklistTorneioItemDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        if (!ModelState.IsValid)
        {
            ViewBag.AdminsTorneio = await _adminTorneioServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }

        try
        {
            var criado = await _checklistServico.Criar(new CriarChecklistTorneioItemDto
            {
                TorneioId = TenantContext.TorneioId,
                Item = dto.Item,
                Data = dto.Data,
                Responsavel = dto.Responsavel,
                Concluido = dto.Concluido
            });
            TempData["Sucesso"] = "Item de checklist criado com sucesso.";
            await RegistrarLog(
                "CriarChecklistWeb",
                $"Item de checklist criado pela retaguarda web | Item: {criado.Item} | Responsavel: {criado.Responsavel ?? "-"} | Concluido: {criado.Concluido}");
            return RedirectToAction(nameof(Checklist), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.AdminsTorneio = await _adminTorneioServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }
    }

    [HttpGet("checklist/{id:guid}/editar")]
    public async Task<IActionResult> EditarChecklist(Guid id)
    {
        var item = await _checklistServico.ObterPorId(id);
        if (item is null)
            return NotFound();

        ViewBag.AdminsTorneio = await _adminTorneioServico.ListarPorTorneio(TenantContext.TorneioId);
        return View(new AtualizarChecklistTorneioItemDto
        {
            Item = item.Item,
            Data = item.Data,
            Responsavel = item.Responsavel,
            Concluido = item.Concluido
        });
    }

    [HttpPost("checklist/{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarChecklist(Guid id, AtualizarChecklistTorneioItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AdminsTorneio = await _adminTorneioServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }

        try
        {
            await _checklistServico.Atualizar(id, dto);
            TempData["Sucesso"] = "Item de checklist atualizado.";
            await RegistrarLog(
                "AtualizarChecklistWeb",
                $"Item de checklist atualizado pela retaguarda web | Item: {id} | Descricao: {dto.Item} | Concluido: {dto.Concluido}");
            return RedirectToAction(nameof(Checklist), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.AdminsTorneio = await _adminTorneioServico.ListarPorTorneio(TenantContext.TorneioId);
            return View(dto);
        }
    }

    [HttpPost("checklist/{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverChecklist(Guid id)
    {
        try
        {
            await _checklistServico.Remover(id);
            TempData["Sucesso"] = "Item de checklist removido.";
            await RegistrarLog("RemoverChecklistWeb", $"Item de checklist removido pela retaguarda web | Item: {id}");
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Checklist), new { slug = Slug });
    }

    private async Task RegistrarLog(string acao, string descricao)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Financeiro,
            Acao = acao,
            Descricao = descricao,
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = UsuarioPerfil,
            IpAddress = IpAddress
        });
    }
}
