using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;
using Torneio.Domain.Enums;

namespace Torneio.Application.Services.Implementations;

public class TorneioServico : ITorneioServico
{
    private readonly ITorneioRepositorio _repositorio;
    private readonly IFiscalRepositorio _fiscalRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IPremioRepositorio _premioRepositorio;
    private readonly IItemRepositorio _itemRepositorio;
    private readonly ICapturaRepositorio _capturaRepositorio;
    private readonly IBannerRepositorio _bannerRepositorio;
    private readonly IFileStorage _fileStorage;
    private readonly IValidator<CriarTorneioDto> _validadorCriar;
    private readonly IValidator<AtualizarTorneioDto> _validadorAtualizar;

    public TorneioServico(
        ITorneioRepositorio repositorio,
        IFiscalRepositorio fiscalRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IMembroRepositorio membroRepositorio,
        IPremioRepositorio premioRepositorio,
        IItemRepositorio itemRepositorio,
        ICapturaRepositorio capturaRepositorio,
        IBannerRepositorio bannerRepositorio,
        IFileStorage fileStorage,
        IValidator<CriarTorneioDto> validadorCriar,
        IValidator<AtualizarTorneioDto> validadorAtualizar)
    {
        _repositorio = repositorio;
        _fiscalRepositorio = fiscalRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _membroRepositorio = membroRepositorio;
        _premioRepositorio = premioRepositorio;
        _itemRepositorio = itemRepositorio;
        _capturaRepositorio = capturaRepositorio;
        _bannerRepositorio = bannerRepositorio;
        _fileStorage = fileStorage;
        _validadorCriar = validadorCriar;
        _validadorAtualizar = validadorAtualizar;
    }

    public async Task<TorneioDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<TorneioDto?> ObterPorSlug(string slug)
    {
        var entidade = await _repositorio.ObterPorSlug(slug);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<TorneioDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarTodos();
        return lista.Select(ParaDto);
    }

    public async Task<IEnumerable<TorneioDto>> ListarAtivos()
    {
        var lista = await _repositorio.ListarAtivos();
        return lista.Select(ParaDto);
    }

    public async Task<TorneioDto> Criar(CriarTorneioDto dto)
    {
        await _validadorCriar.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorSlug(dto.Slug);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um torneio com o slug '{dto.Slug}'.");

        var entidade = TorneioEntity.Criar(
            dto.Slug, dto.NomeTorneio,
            dto.LabelEquipe, dto.LabelEquipePlural,
            dto.LabelMembro, dto.LabelMembroPlural,
            dto.LabelSupervisor, dto.LabelSupervisorPlural,
            dto.LabelItem, dto.LabelItemPlural,
            dto.LabelCaptura, dto.LabelCapturaPlural,
            dto.MedidaCaptura,
            dto.ModoSorteio, dto.TipoTorneio,
            dto.UsarFatorMultiplicador, dto.PermitirCapturaOffline, dto.ExibirModuloFinanceiro,
            dto.PermitirRegistroPublicoMembro,
            dto.QtdGanhadores, dto.PremiacaoPorEquipe, dto.PremiacaoPorMembro,
            dto.ApenasMaiorCapturaPorPescador,
            dto.LogoUrl, dto.CorPrimaria);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarTorneioDto dto)
    {
        await _validadorAtualizar.ValidateAndThrowAsync(dto);

        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");

        entidade.AtualizarConfiguracoes(
            dto.NomeTorneio,
            dto.LabelEquipe, dto.LabelEquipePlural,
            dto.LabelMembro, dto.LabelMembroPlural,
            dto.LabelSupervisor, dto.LabelSupervisorPlural,
            dto.LabelItem, dto.LabelItemPlural,
            dto.LabelCaptura, dto.LabelCapturaPlural,
            dto.MedidaCaptura,
            dto.ModoSorteio, dto.UsarFatorMultiplicador,
            dto.PermitirCapturaOffline, dto.ExibirModuloFinanceiro, dto.PermitirRegistroPublicoMembro, dto.QtdGanhadores,
            dto.PremiacaoPorEquipe, dto.PremiacaoPorMembro,
            dto.ApenasMaiorCapturaPorPescador,
            dto.LogoUrl, dto.CorPrimaria);

        await _repositorio.Atualizar(entidade);
    }

    public async Task Ativar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Ativar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Desativar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Desativar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Liberar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Liberar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Finalizar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Finalizar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Reabrir(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Reabrir();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Excluir(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");

        // Coleta todos os caminhos de arquivos antes de deletar do banco
        var urls = new List<string?>();

        urls.Add(entidade.LogoUrl);

        var fiscais  = await _fiscalRepositorio.ListarPorTorneio(id);
        urls.AddRange(fiscais.Select(f => f.FotoUrl));

        var equipes  = await _equipeRepositorio.ListarPorTorneio(id);
        urls.AddRange(equipes.SelectMany(e => new[] { e.FotoUrl, e.FotoCapitaoUrl }));

        var membros  = await _membroRepositorio.ListarPorTorneio(id);
        urls.AddRange(membros.Select(m => m.FotoUrl));

        var itens    = await _itemRepositorio.ListarPorTorneio(id);
        urls.AddRange(itens.Select(i => i.FotoUrl));

        var capturas = await _capturaRepositorio.ListarPorTorneio(id);
        urls.AddRange(capturas.Select(c => c.FotoUrl));

        var banners  = await _bannerRepositorio.ListarTodos();
        urls.AddRange(banners.Where(b => b.TorneioId == id).Select(b => (string?)b.ImagemUrl));

        // Remove do banco (cascade deleta todos os filhos)
        await _repositorio.Remover(entidade.Id);

        // Remove arquivos do storage (erros ignorados — não devem bloquear a exclusão)
        var caminhos = urls
            .Select(u => _fileStorage.UrlParaCaminhoRelativo(u))
            .Where(c => c is not null)
            .Distinct();

        foreach (var caminho in caminhos)
            await _fileStorage.RemoverAsync(caminho!);
    }

    public async Task<TorneioDto> ClonarTorneio(Guid torneioId, string novoSlug, string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoSlug))
            throw new ArgumentException("O slug da nova edição é obrigatório.");
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new ArgumentException("O nome da nova edição é obrigatório.");

        var origem = await _repositorio.ObterPorId(torneioId)
            ?? throw new KeyNotFoundException($"Torneio origem '{torneioId}' não encontrado.");

        var existente = await _repositorio.ObterPorSlug(novoSlug.Trim());
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um torneio com o slug '{novoSlug}'.");

        var novoTorneio = TorneioEntity.Criar(
            novoSlug.Trim(), novoNome.Trim(),
            origem.LabelEquipe, origem.LabelEquipePlural,
            origem.LabelMembro, origem.LabelMembroPlural,
            origem.LabelSupervisor, origem.LabelSupervisorPlural,
            origem.LabelItem, origem.LabelItemPlural,
            origem.LabelCaptura, origem.LabelCapturaPlural,
            origem.MedidaCaptura,
            origem.ModoSorteio, origem.TipoTorneio,
            origem.UsarFatorMultiplicador, origem.PermitirCapturaOffline, origem.ExibirModuloFinanceiro,
            origem.PermitirRegistroPublicoMembro,
            origem.QtdGanhadores, origem.PremiacaoPorEquipe, origem.PremiacaoPorMembro,
            origem.ApenasMaiorCapturaPorPescador,
            origem.LogoUrl, origem.CorPrimaria);

        await _repositorio.Adicionar(novoTorneio);

        // Clona Fiscais
        var fiscais = await _fiscalRepositorio.ListarPorTorneio(torneioId);
        var mapaFiscais = new Dictionary<Guid, Guid>();
        var mapaFiscalEquipes = new Dictionary<Guid, List<Guid>>();
        foreach (var fiscal in fiscais)
        {
            var novoFiscal = Fiscal.Criar(
                novoTorneio.Id, fiscal.Nome, fiscal.Usuario, fiscal.SenhaHash, fiscal.FotoUrl);
            await _fiscalRepositorio.Adicionar(novoFiscal);
            mapaFiscais[fiscal.Id] = novoFiscal.Id;
            mapaFiscalEquipes[fiscal.Id] = fiscal.Equipes.Select(x => x.EquipeId).ToList();
        }

        // Clona Equipes
        var equipes = await _equipeRepositorio.ListarPorTorneio(torneioId);
        var mapaEquipes = new Dictionary<Guid, Guid>();
        foreach (var equipe in equipes)
        {
            var novaEquipe = Equipe.Criar(
                novoTorneio.Id, equipe.Nome, equipe.Capitao,
                equipe.QtdVagas, equipe.FotoUrl, equipe.FotoCapitaoUrl);
            await _equipeRepositorio.Adicionar(novaEquipe);
            mapaEquipes[equipe.Id] = novaEquipe.Id;
        }

        foreach (var fiscal in fiscais)
        {
            if (!mapaFiscais.TryGetValue(fiscal.Id, out var novoFiscalId))
                continue;

            var novoFiscal = await _fiscalRepositorio.ObterComEquipes(novoFiscalId);
            if (novoFiscal is null)
                continue;

            var novasEquipes = mapaFiscalEquipes[fiscal.Id]
                .Where(mapaEquipes.ContainsKey)
                .Select(equipeId => mapaEquipes[equipeId]);

            novoFiscal.DefinirEquipes(novasEquipes);
            await _fiscalRepositorio.Atualizar(novoFiscal);
        }

        // Clona Membros
        var membros = await _membroRepositorio.ListarPorTorneio(torneioId);
        foreach (var membro in membros)
        {
            var novoMembro = Membro.Criar(novoTorneio.Id, membro.Nome, membro.FotoUrl);
            await _membroRepositorio.Adicionar(novoMembro);
        }

        // Clona Prêmios
        var premios = await _premioRepositorio.ListarPorTorneio(torneioId);
        foreach (var premio in premios)
        {
            var novoPremio = Premio.Criar(novoTorneio.Id, premio.Posicao, premio.Descricao);
            await _premioRepositorio.Adicionar(novoPremio);
        }

        return ParaDto(novoTorneio);
    }

    private static TorneioDto ParaDto(TorneioEntity e) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        NomeTorneio = e.NomeTorneio,
        LogoUrl = e.LogoUrl,
        Ativo = e.Ativo,
        Status = e.Status.ToString(),
        LabelEquipe = e.LabelEquipe,
        LabelEquipePlural = e.LabelEquipePlural,
        LabelMembro = e.LabelMembro,
        LabelMembroPlural = e.LabelMembroPlural,
        LabelSupervisor = e.LabelSupervisor,
        LabelSupervisorPlural = e.LabelSupervisorPlural,
        LabelItem = e.LabelItem,
        LabelItemPlural = e.LabelItemPlural,
        LabelCaptura = e.LabelCaptura,
        LabelCapturaPlural = e.LabelCapturaPlural,
        UsarFatorMultiplicador = e.UsarFatorMultiplicador,
        MedidaCaptura = e.MedidaCaptura,
        PermitirCapturaOffline = e.PermitirCapturaOffline,
        ExibirModuloFinanceiro = e.ExibirModuloFinanceiro,
        PermitirRegistroPublicoMembro = e.PermitirRegistroPublicoMembro,
        ValorPorMembro = e.ValorPorMembro,
        QuantidadeParcelas = e.QuantidadeParcelas,
        DataPrimeiroVencimento = e.DataPrimeiroVencimento,
        ModoSorteio = e.ModoSorteio.ToString(),
        QtdGanhadores = e.QtdGanhadores,
        PremiacaoPorEquipe = e.PremiacaoPorEquipe,
        PremiacaoPorMembro = e.PremiacaoPorMembro,
        ApenasMaiorCapturaPorPescador = e.ApenasMaiorCapturaPorPescador,
        TipoTorneio = e.TipoTorneio.ToString(),
        CriadoEm = e.CriadoEm,
        CorPrimaria = e.CorPrimaria,
    };

    public async Task<IEnumerable<TorneioResumoDto>> ListarRecentes(int limite = 5)
    {
        var lista = await _repositorio.ListarRecentes(limite);
        return lista.Select(ParaResumoDto);
    }

    public async Task<IEnumerable<TorneioResumoDto>> BuscarPorTexto(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return [];
        var lista = await _repositorio.BuscarPorTexto(q.Trim());
        return lista.Select(ParaResumoDto);
    }

    private static TorneioResumoDto ParaResumoDto(TorneioEntity e) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        NomeTorneio = e.NomeTorneio,
        LogoUrl = e.LogoUrl,
        Status = e.Status.ToString(),
        Ativo = e.Ativo,
        CriadoEm = e.CriadoEm,
    };
}
