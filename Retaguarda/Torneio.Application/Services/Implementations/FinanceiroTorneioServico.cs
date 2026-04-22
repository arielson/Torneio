using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class FinanceiroTorneioServico : IFinanceiroTorneioServico
{
    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IParcelaTorneioRepositorio _parcelaRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IAdminTorneioRepositorio _adminRepositorio;
    private readonly ICustoTorneioRepositorio _custoRepositorio;
    private readonly IProdutoExtraTorneioRepositorio _produtoRepositorio;
    private readonly IProdutoExtraMembroRepositorio _produtoMembroRepositorio;
    private readonly IDoacaoPatrocinadorRepositorio _doacaoRepositorio;

    public FinanceiroTorneioServico(
        ITorneioRepositorio torneioRepositorio,
        IMembroRepositorio membroRepositorio,
        IParcelaTorneioRepositorio parcelaRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IAdminTorneioRepositorio adminRepositorio,
        ICustoTorneioRepositorio custoRepositorio,
        IProdutoExtraTorneioRepositorio produtoRepositorio,
        IProdutoExtraMembroRepositorio produtoMembroRepositorio,
        IDoacaoPatrocinadorRepositorio doacaoRepositorio)
    {
        _torneioRepositorio = torneioRepositorio;
        _membroRepositorio = membroRepositorio;
        _parcelaRepositorio = parcelaRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _adminRepositorio = adminRepositorio;
        _custoRepositorio = custoRepositorio;
        _produtoRepositorio = produtoRepositorio;
        _produtoMembroRepositorio = produtoMembroRepositorio;
        _doacaoRepositorio = doacaoRepositorio;
    }

    public async Task<TorneioFinanceiroConfigDto> ObterConfiguracao(Guid torneioId)
    {
        var torneio = await _torneioRepositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException($"Torneio '{torneioId}' nao encontrado.");

        return new TorneioFinanceiroConfigDto
        {
            TorneioId = torneioId,
            ValorPorMembro = torneio.ValorPorMembro,
            QuantidadeParcelas = torneio.QuantidadeParcelas,
            DataPrimeiroVencimento = torneio.DataPrimeiroVencimento,
            TaxaInscricaoValor = torneio.TaxaInscricaoValor,
            DataVencimentoTaxaInscricao = torneio.DataVencimentoTaxaInscricao,
            PossuiConfiguracaoAnterior = PossuiConfiguracaoAnterior(torneio)
        };
    }

    public async Task AtualizarConfiguracao(Guid torneioId, AtualizarTorneioFinanceiroDto dto)
    {
        var torneio = await _torneioRepositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException($"Torneio '{torneioId}' nao encontrado.");

        if (PossuiConfiguracaoAnterior(torneio) && ConfiguracaoFoiAlterada(torneio, dto) && !dto.ConfirmarSubstituicao)
            throw new InvalidOperationException("Ja existe uma configuracao financeira salva. Confirme a substituicao para limpar a configuracao anterior e criar uma nova.");

        torneio.AtualizarFinanceiro(
            dto.ValorPorMembro,
            dto.QuantidadeParcelas,
            dto.DataPrimeiroVencimento,
            dto.TaxaInscricaoValor,
            dto.DataVencimentoTaxaInscricao);

        await _torneioRepositorio.Atualizar(torneio);
        await SincronizarParcelas(torneioId);
    }

    public async Task SincronizarParcelas(Guid torneioId)
    {
        await SincronizarParcelasInterno(torneioId, null, false);
    }

    public async Task SincronizarParcelas(Guid torneioId, IReadOnlyCollection<Guid> membroIds, bool somenteNovos = false)
    {
        await SincronizarParcelasInterno(torneioId, membroIds, somenteNovos);
    }

    private async Task SincronizarParcelasInterno(Guid torneioId, IReadOnlyCollection<Guid>? membroIds, bool somenteNovos)
    {
        var torneio = await _torneioRepositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException($"Torneio '{torneioId}' nao encontrado.");

        var existentes = (await _parcelaRepositorio.ListarPorTorneio(torneioId)).ToList();
        var membros = (await _membroRepositorio.ListarPorTorneio(torneioId)).OrderBy(x => x.Nome).ToList();
        var produtos = (await _produtoRepositorio.ListarPorTorneio(torneioId)).ToDictionary(x => x.Id);
        var adesoes = (await _produtoMembroRepositorio.ListarPorTorneio(torneioId))
            .Where(x => x.Ativo)
            .ToList();

        if (membroIds is { Count: > 0 })
            membros = membros.Where(x => membroIds.Contains(x.Id)).ToList();

        if (somenteNovos)
        {
            var membrosComCobrancasBase = existentes
                .Where(x => x.TipoParcela is TipoParcelaTorneio.Mensalidade or TipoParcelaTorneio.TaxaInscricao)
                .Select(x => x.MembroId)
                .Distinct()
                .ToHashSet();
            membros = membros.Where(x => !membrosComCobrancasBase.Contains(x.Id)).ToList();
        }

        var mapaExistentes = existentes.ToDictionary(ChaveParcela);
        var chavesDesejadas = new HashSet<string>();

        await SincronizarMensalidades(torneio, membros, mapaExistentes, chavesDesejadas);
        await SincronizarTaxaInscricao(torneio, membros, mapaExistentes, chavesDesejadas);

        var sincronizacaoCompleta = membroIds is null;
        if (sincronizacaoCompleta)
            await SincronizarProdutosExtras(torneioId, adesoes, produtos, mapaExistentes, chavesDesejadas);

        var membrosAlvo = membros.Select(x => x.Id).ToHashSet();
        var obsoletas = sincronizacaoCompleta
            ? existentes.Where(x => !chavesDesejadas.Contains(ChaveParcela(x))).ToList()
            : existentes.Where(x =>
                    membrosAlvo.Contains(x.MembroId)
                    && x.TipoParcela is TipoParcelaTorneio.Mensalidade or TipoParcelaTorneio.TaxaInscricao
                    && !chavesDesejadas.Contains(ChaveParcela(x)))
                .ToList();
        var obsoletasPagas = obsoletas.Where(x => x.Pago).ToList();
        if (obsoletasPagas.Count > 0)
            throw new InvalidOperationException("Nao foi possivel regenerar porque existem cobrancas pagas fora da nova configuracao.");

        if (obsoletas.Count > 0)
            await _parcelaRepositorio.RemoverRange(obsoletas);
    }

    public async Task<IEnumerable<ParcelaTorneioDto>> ListarParcelas(
        Guid torneioId,
        Guid? membroId = null,
        bool somenteInadimplentes = false,
        bool somenteNaoPagas = false,
        string? tipoParcela = null)
    {
        var membros = (await _membroRepositorio.ListarPorTorneio(torneioId)).ToDictionary(x => x.Id, x => x.Nome);
        var parcelas = (await _parcelaRepositorio.ListarPorTorneio(torneioId)).AsEnumerable();

        if (membroId.HasValue)
            parcelas = parcelas.Where(x => x.MembroId == membroId.Value);

        if (TryResolverTipoParcela(tipoParcela, out var tipoFiltro))
            parcelas = parcelas.Where(x => x.TipoParcela == tipoFiltro);

        var dtos = parcelas.Select(x => ParaParcelaDto(x, membros)).ToList();

        if (somenteNaoPagas)
            dtos = dtos.Where(x => !x.Pago).ToList();

        if (somenteInadimplentes)
            dtos = dtos.Where(x => x.Inadimplente).ToList();

        return dtos
            .OrderBy(x => x.NomeMembro)
            .ThenBy(x => x.TipoParcela)
            .ThenBy(x => x.NumeroParcela)
            .ThenBy(x => x.Vencimento);
    }

    public async Task<ParcelaTorneioDto?> ObterParcela(Guid id)
    {
        var parcela = await _parcelaRepositorio.ObterPorId(id);
        if (parcela is null)
            return null;

        var membro = await _membroRepositorio.ObterPorId(parcela.MembroId);
        return ParaParcelaDto(parcela, new Dictionary<Guid, string>
        {
            [parcela.MembroId] = membro?.Nome ?? string.Empty
        });
    }

    public async Task AtualizarParcela(Guid id, AtualizarParcelaTorneioDto dto)
    {
        var parcela = await _parcelaRepositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Parcela '{id}' nao encontrada.");

        parcela.AtualizarVencimento(dto.Vencimento, editadoManual: true);
        parcela.AtualizarObservacao(dto.Observacao);
        await _parcelaRepositorio.Atualizar(parcela);
    }

    public async Task AtualizarPagamento(Guid id, AtualizarPagamentoParcelaDto dto)
    {
        var parcela = await _parcelaRepositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Parcela '{id}' nao encontrada.");

        if (dto.Pago)
            parcela.MarcarComoPago(dto.DataPagamento);
        else
            parcela.DesmarcarPagamento();

        await _parcelaRepositorio.Atualizar(parcela);
    }

    public async Task AtualizarComprovante(Guid id, string nomeArquivo, string url, string? contentType, string usuarioNome)
    {
        var parcela = await _parcelaRepositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Parcela '{id}' nao encontrada.");

        parcela.AtualizarComprovante(nomeArquivo, url, contentType, usuarioNome);
        await _parcelaRepositorio.Atualizar(parcela);
    }

    public async Task<IndicadoresFinanceiroDto> ObterIndicadores(Guid torneioId)
    {
        var torneio = await _torneioRepositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException($"Torneio '{torneioId}' nao encontrado.");
        var membros = (await _membroRepositorio.ListarPorTorneio(torneioId)).ToList();
        var equipes = (await _equipeRepositorio.ListarPorTorneio(torneioId)).ToList();
        var admins = (await _adminRepositorio.ListarPorTorneio(torneioId)).ToList();
        var parcelas = (await _parcelaRepositorio.ListarPorTorneio(torneioId)).ToList();
        var custos = (await _custoRepositorio.ListarPorTorneio(torneioId)).ToList();
        var produtos = (await _produtoRepositorio.ListarPorTorneio(torneioId)).ToList();
        var adesoes = (await _produtoMembroRepositorio.ListarPorTorneio(torneioId)).Where(x => x.Ativo).ToList();
        var doacoes = (await _doacaoRepositorio.ListarPorTorneio(torneioId)).ToList();

        var custoEmbarcacoes = equipes
            .Where(x => x.StatusFinanceiro != StatusEmbarcacaoFinanceira.Cancelada)
            .Sum(x => x.Custo);
        var custoTotal = custos.Sum(x => x.ValorTotal) + custoEmbarcacoes;
        var arrecadacaoBase = membros.Count * torneio.ValorPorMembro;
        var receitaTaxaInscricao = membros.Count * torneio.TaxaInscricaoValor;
        var receitaExtras = adesoes.Sum(x => x.ValorCobrado);
        var arrecadacaoPrevista = arrecadacaoBase + receitaTaxaInscricao + receitaExtras;
        var receitaDoacoes = doacoes
            .Where(x => x.Tipo == TipoDoacaoPatrocinador.Dinheiro)
            .Sum(x => x.Valor ?? 0);
        var receitaPrevista = arrecadacaoPrevista + receitaDoacoes;
        var abertas = parcelas.Where(x => !x.Pago).ToList();
        var inadimplentes = abertas.Where(x => x.Vencimento.Date < DateTime.UtcNow.Date).ToList();
        var quantidadeCustos = custos.Count + equipes.Count(x => x.Custo > 0 && x.StatusFinanceiro != StatusEmbarcacaoFinanceira.Cancelada);

        return new IndicadoresFinanceiroDto
        {
            QuantidadeMembros = membros.Count,
            QuantidadeEquipes = equipes.Count,
            QuantidadeAdministradores = admins.Count,
            CustoTotalTorneio = custoTotal,
            ValorPorMembro = torneio.ValorPorMembro,
            TaxaInscricaoValor = torneio.TaxaInscricaoValor,
            QuantidadeParcelas = torneio.QuantidadeParcelas,
            ArrecadacaoPrevista = arrecadacaoPrevista,
            ReceitaPrevista = receitaPrevista,
            ReceitaExtrasPrevista = receitaExtras,
            ReceitaDoacoesPatrocinadores = receitaDoacoes,
            SaldoProjetado = receitaPrevista - custoTotal,
            ParcelasInadimplentes = inadimplentes.Count,
            ValorEmAberto = abertas.Sum(x => x.Valor),
            EmbarcacoesConfirmadas = equipes.Count(x => x.StatusFinanceiro == StatusEmbarcacaoFinanceira.Confirmada),
            QuantidadeCustosLancados = quantidadeCustos,
            QuantidadeProdutosExtras = produtos.Count,
            QuantidadeDoacoesPatrocinadores = doacoes.Count
        };
    }

    public async Task ValidarRemocaoMembro(Guid membroId)
    {
        var parcelas = (await _parcelaRepositorio.ListarPorMembro(membroId)).ToList();
        if (parcelas.Any(x => x.Pago))
            throw new InvalidOperationException("Nao e possivel remover o pescador porque existem cobrancas pagas vinculadas a ele.");
    }

    public bool ConfiguracaoPossuiDados(TorneioFinanceiroConfigDto config) =>
        config.ValorPorMembro > 0
        || config.QuantidadeParcelas > 0
        || config.DataPrimeiroVencimento.HasValue
        || config.TaxaInscricaoValor > 0
        || config.DataVencimentoTaxaInscricao.HasValue;

    private async Task SincronizarMensalidades(
        TorneioEntity torneio,
        List<Membro> membros,
        Dictionary<string, ParcelaTorneio> mapaExistentes,
        HashSet<string> chavesDesejadas)
    {
        var existentesMensalidade = mapaExistentes.Values.Where(x => x.TipoParcela == TipoParcelaTorneio.Mensalidade).ToList();

        if (torneio.QuantidadeParcelas == 0)
        {
            if (existentesMensalidade.Any(x => x.Pago))
                throw new InvalidOperationException("Nao e possivel zerar a quantidade de parcelas porque ja existem mensalidades pagas.");
            return;
        }

        var maiorParcelaPaga = existentesMensalidade.Where(x => x.Pago).Select(x => x.NumeroParcela).DefaultIfEmpty(0).Max();
        if (maiorParcelaPaga > torneio.QuantidadeParcelas)
            throw new InvalidOperationException("Nao e possivel reduzir a quantidade de parcelas abaixo da ultima mensalidade ja paga.");

        var valores = CalcularValoresParcelas(torneio.ValorPorMembro, torneio.QuantidadeParcelas);
        var primeiroVencimento = torneio.DataPrimeiroVencimento ?? DateTime.UtcNow.Date;

        foreach (var membro in membros)
        {
            for (var numero = 1; numero <= torneio.QuantidadeParcelas; numero++)
            {
                var chave = ChaveParcela(membro.Id, TipoParcelaTorneio.Mensalidade, numero, null);
                chavesDesejadas.Add(chave);
                var valor = valores[numero - 1];
                var vencimentoPadrao = primeiroVencimento.AddMonths(numero - 1);

                if (mapaExistentes.TryGetValue(chave, out var existente))
                {
                    existente.AtualizarValor(valor);
                    existente.AtualizarDescricao($"Mensalidade {numero}");
                    if (!existente.VencimentoEditadoManual)
                        existente.AtualizarVencimento(vencimentoPadrao, editadoManual: false);
                    await _parcelaRepositorio.Atualizar(existente);
                }
                else
                {
                    var nova = ParcelaTorneio.Criar(
                        torneio.Id,
                        membro.Id,
                        TipoParcelaTorneio.Mensalidade,
                        numero,
                        $"Mensalidade {numero}",
                        valor,
                        vencimentoPadrao);
                    await _parcelaRepositorio.Adicionar(nova);
                }
            }
        }
    }

    private async Task SincronizarTaxaInscricao(
        TorneioEntity torneio,
        List<Membro> membros,
        Dictionary<string, ParcelaTorneio> mapaExistentes,
        HashSet<string> chavesDesejadas)
    {
        var existentesTaxa = mapaExistentes.Values.Where(x => x.TipoParcela == TipoParcelaTorneio.TaxaInscricao).ToList();

        if (torneio.TaxaInscricaoValor <= 0)
        {
            if (existentesTaxa.Any(x => x.Pago))
                throw new InvalidOperationException("Nao e possivel remover a taxa de inscricao porque ja existem taxas pagas.");
            return;
        }

        var vencimento = torneio.DataVencimentoTaxaInscricao
            ?? torneio.DataPrimeiroVencimento
            ?? DateTime.UtcNow.Date;

        foreach (var membro in membros)
        {
            var chave = ChaveParcela(membro.Id, TipoParcelaTorneio.TaxaInscricao, 1, null);
            chavesDesejadas.Add(chave);

            if (mapaExistentes.TryGetValue(chave, out var existente))
            {
                existente.AtualizarValor(torneio.TaxaInscricaoValor);
                existente.AtualizarDescricao("Taxa de inscricao");
                if (!existente.VencimentoEditadoManual)
                    existente.AtualizarVencimento(vencimento, editadoManual: false);
                await _parcelaRepositorio.Atualizar(existente);
            }
            else
            {
                var nova = ParcelaTorneio.Criar(
                    torneio.Id,
                    membro.Id,
                    TipoParcelaTorneio.TaxaInscricao,
                    1,
                    "Taxa de inscricao",
                    torneio.TaxaInscricaoValor,
                    vencimento);
                await _parcelaRepositorio.Adicionar(nova);
            }
        }
    }

    private async Task SincronizarProdutosExtras(
        Guid torneioId,
        List<ProdutoExtraMembro> adesoes,
        Dictionary<Guid, ProdutoExtraTorneio> produtos,
        Dictionary<string, ParcelaTorneio> mapaExistentes,
        HashSet<string> chavesDesejadas)
    {
        foreach (var adesao in adesoes)
        {
            if (!produtos.TryGetValue(adesao.ProdutoExtraTorneioId, out var produto))
                continue;

            var chave = ChaveParcela(adesao.MembroId, TipoParcelaTorneio.ProdutoExtra, 1, adesao.Id);
            chavesDesejadas.Add(chave);
            var vencimento = DateTime.UtcNow.Date;

            if (mapaExistentes.TryGetValue(chave, out var existente))
            {
                existente.AtualizarValor(adesao.ValorCobrado);
                existente.AtualizarDescricao($"Extra: {produto.Nome} x {adesao.Quantidade:0.##}");
                await _parcelaRepositorio.Atualizar(existente);
            }
            else
            {
                var nova = ParcelaTorneio.Criar(
                    torneioId,
                    adesao.MembroId,
                    TipoParcelaTorneio.ProdutoExtra,
                    1,
                    $"Extra: {produto.Nome} x {adesao.Quantidade:0.##}",
                    adesao.ValorCobrado,
                    vencimento,
                    adesao.Id);
                await _parcelaRepositorio.Adicionar(nova);
            }
        }
    }

    private static ParcelaTorneioDto ParaParcelaDto(ParcelaTorneio parcela, IReadOnlyDictionary<Guid, string> membros)
    {
        var nomeMembro = membros.TryGetValue(parcela.MembroId, out var nome) ? nome : string.Empty;
        var inadimplente = !parcela.Pago && parcela.Vencimento.Date < DateTime.UtcNow.Date;
        return new ParcelaTorneioDto
        {
            Id = parcela.Id,
            TorneioId = parcela.TorneioId,
            MembroId = parcela.MembroId,
            NomeMembro = nomeMembro,
            TipoParcela = parcela.TipoParcela.ToString(),
            Descricao = parcela.Descricao,
            NumeroParcela = parcela.NumeroParcela,
            Valor = parcela.Valor,
            Vencimento = parcela.Vencimento,
            VencimentoEditadoManual = parcela.VencimentoEditadoManual,
            Pago = parcela.Pago,
            DataPagamento = parcela.DataPagamento,
            Observacao = parcela.Observacao,
            Inadimplente = inadimplente,
            ComprovanteNomeArquivo = parcela.ComprovanteNomeArquivo,
            ComprovanteDataUpload = parcela.ComprovanteDataUpload,
            ComprovanteUsuarioNome = parcela.ComprovanteUsuarioNome,
            ComprovanteUrl = parcela.ComprovanteUrl,
            ComprovanteContentType = parcela.ComprovanteContentType
        };
    }

    private static List<decimal> CalcularValoresParcelas(decimal total, int quantidadeParcelas)
    {
        if (quantidadeParcelas <= 0)
            return [];

        var baseValor = Math.Round(total / quantidadeParcelas, 2, MidpointRounding.AwayFromZero);
        var valores = Enumerable.Repeat(baseValor, quantidadeParcelas).ToList();
        var soma = valores.Sum();
        var diferenca = total - soma;
        if (valores.Count > 0)
            valores[^1] += diferenca;
        return valores;
    }

    private static string ChaveParcela(ParcelaTorneio parcela) =>
        ChaveParcela(parcela.MembroId, parcela.TipoParcela, parcela.NumeroParcela, parcela.ReferenciaId);

    private static string ChaveParcela(Guid membroId, TipoParcelaTorneio tipo, int numero, Guid? referenciaId) =>
        $"{membroId}:{tipo}:{numero}:{referenciaId}";

    private static bool TryResolverTipoParcela(string? tipoParcela, out TipoParcelaTorneio tipo)
    {
        tipo = default;
        if (string.IsNullOrWhiteSpace(tipoParcela))
            return false;

        return tipoParcela.Trim().ToLowerInvariant() switch
        {
            "mensalidade" or "parcela" => ResolveTipo(TipoParcelaTorneio.Mensalidade, out tipo),
            "taxainscricao" or "taxa_inscricao" or "taxa-inscricao" or "inscricao" or "inscrição" => ResolveTipo(TipoParcelaTorneio.TaxaInscricao, out tipo),
            "produtoextra" or "produto_extra" or "produto-extra" or "produto extra" => ResolveTipo(TipoParcelaTorneio.ProdutoExtra, out tipo),
            _ when Enum.TryParse<TipoParcelaTorneio>(tipoParcela, true, out var parsed) => ResolveTipo(parsed, out tipo),
            _ => false
        };
    }

    private static bool ResolveTipo(TipoParcelaTorneio value, out TipoParcelaTorneio tipo)
    {
        tipo = value;
        return true;
    }

    private static bool PossuiConfiguracaoAnterior(TorneioEntity torneio) =>
        torneio.ValorPorMembro > 0
        || torneio.QuantidadeParcelas > 0
        || torneio.DataPrimeiroVencimento.HasValue
        || torneio.TaxaInscricaoValor > 0
        || torneio.DataVencimentoTaxaInscricao.HasValue;

    private static bool ConfiguracaoFoiAlterada(TorneioEntity torneio, AtualizarTorneioFinanceiroDto dto) =>
        torneio.ValorPorMembro != dto.ValorPorMembro
        || torneio.QuantidadeParcelas != dto.QuantidadeParcelas
        || torneio.DataPrimeiroVencimento?.Date != dto.DataPrimeiroVencimento?.Date
        || torneio.TaxaInscricaoValor != dto.TaxaInscricaoValor
        || torneio.DataVencimentoTaxaInscricao?.Date != dto.DataVencimentoTaxaInscricao?.Date;
}
