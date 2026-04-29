using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.DTOs.Patrocinador;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure.Services;

public class RelatorioServico : IRelatorioServico
{
    private readonly ICapturaServico _capturaServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly IPatrocinadorServico _patrocinadorServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ITenantContext _tenant;
    private readonly StorageOptions _storage;

    public RelatorioServico(
        ICapturaServico capturaServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        IPatrocinadorServico patrocinadorServico,
        ITorneioServico torneioServico,
        ITenantContext tenant,
        IOptions<StorageOptions> storage)
    {
        _capturaServico = capturaServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _patrocinadorServico = patrocinadorServico;
        _torneioServico = torneioServico;
        _tenant = tenant;
        _storage = storage.Value;
    }

    public async Task<byte[]> GerarRelatorioEquipe(Guid equipeId, bool analitico)
    {
        var torneio = await _torneioServico.ObterPorId(_tenant.TorneioId);
        var equipe = await _equipeServico.ObterPorId(equipeId);
        var capturas = (await _capturaServico.ListarPorEquipe(equipeId)).ToList();
        var patrocinadores = await ObterPatrocinadoresRelatorio();
        var equipesRodape = equipe is null ? new List<EquipeDto>() : await ObterEquipesRelatorio([equipe.Id]);

        if (torneio is null || equipe is null)
            throw new InvalidOperationException("Dados não encontrados para geração do relatório.");

        var totalPontos = capturas.Sum(c => c.Pontuacao);
        var usarFator = torneio.UsarFatorMultiplicador;
        var titulo = $"Relatório de {torneio.LabelEquipe} — {(analitico ? "Analítico" : "Sintético")}";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => AdicionarCabecalhoRelatorio(header, torneio, titulo));

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Item().Text($"{torneio.LabelEquipe}: {equipe.Nome}").Bold().FontSize(12);
                    col.Item().Text($"Capitão: {equipe.Capitao}").FontSize(10);
                    col.Item().PaddingTop(4).Text($"Total de pontos: {totalPontos:F2}").Bold();
                    col.Item().PaddingTop(8).Text($"Capturas ({capturas.Count}):").Bold();

                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(30);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(3);
                            cols.ConstantColumn(60);
                            if (usarFator) cols.ConstantColumn(50);
                            cols.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelItem).Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelMembro).Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"Medida ({torneio.MedidaCaptura})").Bold();
                            if (usarFator) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Fator").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Pontos").Bold();
                        });

                        var idx = 1;
                        foreach (var c in capturas.OrderBy(x => x.NomeMembro).ThenBy(x => x.DataHora))
                        {
                            var bg = idx % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(idx.ToString());
                            table.Cell().Background(bg).Padding(4).Text(c.NomeItem);
                            table.Cell().Background(bg).Padding(4).Text(c.NomeMembro);
                            table.Cell().Background(bg).Padding(4).Text($"{c.TamanhoMedida:F1}");
                            if (usarFator) table.Cell().Background(bg).Padding(4).Text(c.FatorMultiplicador > 1m ? c.FatorMultiplicador.ToString("F2") : "—");
                            table.Cell().Background(bg).Padding(4).Text($"{c.Pontuacao:F2}");
                            idx++;
                        }
                    });

                    if (analitico && capturas.Any())
                    {
                        col.Item().PaddingTop(16).Text("Fotos das Capturas:").Bold();
                        AdicionarFotos(col, capturas);
                    }

                });

                page.Footer().Element(footer => AdicionarRodapeRelatorio(footer, equipesRodape, patrocinadores));
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> GerarRelatorioMembro(Guid membroId, bool analitico)
    {
        var torneio = await _torneioServico.ObterPorId(_tenant.TorneioId);
        var membro = await _membroServico.ObterPorId(membroId);
        var capturas = (await _capturaServico.ListarPorMembro(membroId)).ToList();
        var patrocinadores = await ObterPatrocinadoresRelatorio();
        var equipesRodape = await ObterEquipesRelatorio(capturas.Select(c => c.EquipeId));

        if (torneio is null || membro is null)
            throw new InvalidOperationException("Dados não encontrados para geração do relatório.");

        var totalPontos = capturas.Sum(c => c.Pontuacao);
        var usarFator = torneio.UsarFatorMultiplicador;
        var titulo = $"Relatório de {torneio.LabelMembro} — {(analitico ? "Analítico" : "Sintético")}";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => AdicionarCabecalhoRelatorio(header, torneio, titulo));

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Item().Text($"{torneio.LabelMembro}: {membro.Nome}").Bold().FontSize(12);
                    col.Item().PaddingTop(4).Text($"Total de pontos: {totalPontos:F2}").Bold();
                    col.Item().PaddingTop(8).Text($"Capturas ({capturas.Count}):").Bold();

                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(30);
                            cols.RelativeColumn(4);
                            cols.ConstantColumn(70);
                            if (usarFator) cols.ConstantColumn(50);
                            cols.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelItem).Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"Medida ({torneio.MedidaCaptura})").Bold();
                            if (usarFator) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Fator").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Pontos").Bold();
                        });

                        var idx = 1;
                        foreach (var c in capturas.OrderBy(x => x.DataHora))
                        {
                            var bg = idx % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(idx.ToString());
                            table.Cell().Background(bg).Padding(4).Text(c.NomeItem);
                            table.Cell().Background(bg).Padding(4).Text($"{c.TamanhoMedida:F1}");
                            if (usarFator) table.Cell().Background(bg).Padding(4).Text(c.FatorMultiplicador > 1m ? c.FatorMultiplicador.ToString("F2") : "—");
                            table.Cell().Background(bg).Padding(4).Text($"{c.Pontuacao:F2}");
                            idx++;
                        }
                    });

                    if (analitico && capturas.Any())
                    {
                        col.Item().PaddingTop(16).Text("Fotos das Capturas:").Bold();
                        AdicionarFotos(col, capturas);
                    }

                });

                page.Footer().Element(footer => AdicionarRodapeRelatorio(footer, equipesRodape, patrocinadores));
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> GerarRelatorioGanhadores(
        int quantidadeEquipes,
        int quantidadeMembrosPontuacao,
        int quantidadeMembrosMaiorCaptura,
        bool exibirPescadoresDasEmbarcacoes,
        bool analitico)
    {
        if (quantidadeEquipes is < 0 or > 999)
            throw new InvalidOperationException("A quantidade de embarcações deve estar entre 0 e 999.");
        if (quantidadeMembrosPontuacao is < 0 or > 999)
            throw new InvalidOperationException("A quantidade de pescadores por pontuação deve estar entre 0 e 999.");
        if (quantidadeMembrosMaiorCaptura is < 0 or > 999)
            throw new InvalidOperationException("A quantidade de pescadores por maior captura deve estar entre 0 e 999.");

        var torneio = await _torneioServico.ObterPorId(_tenant.TorneioId);
        if (torneio is null)
            throw new InvalidOperationException("Dados nao encontrados para geração do relatório.");

        var patrocinadores = await ObterPatrocinadoresRelatorio();
        var todasCapturas = (await _capturaServico.ListarTodos())
            .Where(c => !c.Invalidada)
            .ToList();
        var capturasPontuacao = torneio.ApenasMaiorCapturaPorPescador
            ? todasCapturas
                .GroupBy(c => c.MembroId)
                .Select(g => g.OrderByDescending(c => c.Pontuacao).First())
                .ToList()
            : todasCapturas;
        var exibirMaiorCaptura = string.Equals(torneio.TipoTorneio, nameof(TipoTorneio.Pesca), StringComparison.OrdinalIgnoreCase);
        var membros = (await _membroServico.ListarTodos()).ToList();

        var equipes = quantidadeEquipes > 0
            ? (await _equipeServico.ListarTodos())
                .Select(e => new ResumoEquipeGanhadora
                {
                    Posicao = 0,
                    EquipeId = e.Id,
                    NomeEquipe = e.Nome,
                    Capitao = e.Capitao,
                    Pescadores = e.MembroIds
                        .Select(membroId => membros.FirstOrDefault(m => m.Id == membroId)?.Nome)
                        .Where(nome => !string.IsNullOrWhiteSpace(nome))
                        .Cast<string>()
                        .OrderBy(nome => nome)
                        .ToList(),
                    TotalPontos = capturasPontuacao.Where(c => c.EquipeId == e.Id).Sum(c => c.Pontuacao),
                    PrimeiraCaptura = capturasPontuacao.Where(c => c.EquipeId == e.Id).Select(c => c.DataHora).DefaultIfEmpty(DateTime.MaxValue).Min()
                })
                .OrderByDescending(x => x.TotalPontos)
                .ThenBy(x => x.PrimeiraCaptura)
                .ThenBy(x => x.NomeEquipe)
                .Take(quantidadeEquipes)
                .Select((x, i) =>
                {
                    x.Posicao = i + 1;
                    return x;
                })
                .ToList()
            : [];

        var membrosPontuacao = quantidadeMembrosPontuacao > 0
            ? (await _membroServico.ListarTodos())
                .Select(m => new ResumoMembroGanhador
                {
                    Posicao = 0,
                    MembroId = m.Id,
                    NomeMembro = m.Nome,
                    TotalPontos = capturasPontuacao.Where(c => c.MembroId == m.Id).Sum(c => c.Pontuacao),
                    PrimeiraCaptura = capturasPontuacao.Where(c => c.MembroId == m.Id).Select(c => c.DataHora).DefaultIfEmpty(DateTime.MaxValue).Min()
                })
                .OrderByDescending(x => x.TotalPontos)
                .ThenBy(x => x.PrimeiraCaptura)
                .ThenBy(x => x.NomeMembro)
                .Take(quantidadeMembrosPontuacao)
                .Select((x, i) =>
                {
                    x.Posicao = i + 1;
                    return x;
                })
                .ToList()
            : [];

        var membrosMaiorCaptura = exibirMaiorCaptura && quantidadeMembrosMaiorCaptura > 0
            ? todasCapturas
                .GroupBy(c => c.MembroId)
                .Select(g =>
                {
                    var maior = g
                        .OrderByDescending(c => c.TamanhoMedida)
                        .ThenBy(c => c.DataHora)
                        .First();
                    return new ResumoMembroGanhador
                    {
                        Posicao = 0,
                        MembroId = maior.MembroId,
                        NomeMembro = maior.NomeMembro,
                        MaiorCaptura = maior.TamanhoMedida,
                        NomeItemMaiorCaptura = maior.NomeItem,
                        PrimeiraCaptura = maior.DataHora
                    };
                })
                .OrderByDescending(x => x.MaiorCaptura ?? 0m)
                .ThenBy(x => x.PrimeiraCaptura)
                .ThenBy(x => x.NomeMembro)
                .Take(quantidadeMembrosMaiorCaptura)
                .Select((x, i) =>
                {
                    x.Posicao = i + 1;
                    return x;
                })
                .ToList()
            : [];

        var titulo = $"Relatorio dos Ganhadores - {(analitico ? "Analitico" : "Sintetico")}";
        var usarFator = torneio.UsarFatorMultiplicador;
        var equipesRodape = await ObterEquipesRelatorio(equipes.Select(e => e.EquipeId));

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => AdicionarCabecalhoRelatorio(
                    header,
                    torneio,
                    titulo,
                    $"Embarcacoes: {quantidadeEquipes} | {torneio.LabelMembroPlural} por pontuacao: {quantidadeMembrosPontuacao}" +
                    (exibirMaiorCaptura ? $" | {torneio.LabelMembroPlural} por maior captura: {quantidadeMembrosMaiorCaptura}" : string.Empty)));

                page.Content().PaddingTop(12).Column(col =>
                {
                    if (!equipes.Any() && !membrosPontuacao.Any() && !membrosMaiorCaptura.Any())
                    {
                        col.Item().Text("Nenhum ganhador foi encontrado com base nas capturas atuais.")
                            .FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        AdicionarResumoEquipes(col, equipes, torneio, exibirPescadoresDasEmbarcacoes);
                        AdicionarResumoMembrosPontuacao(col, membrosPontuacao, torneio);
                        if (exibirMaiorCaptura)
                            AdicionarResumoMembrosMaiorCaptura(col, membrosMaiorCaptura, torneio);

                        if (analitico)
                        {
                            AdicionarDetalhamentoEquipes(col, equipes, capturasPontuacao, torneio, usarFator, exibirPescadoresDasEmbarcacoes);
                            AdicionarDetalhamentoMembros(col, membrosPontuacao, capturasPontuacao, torneio, usarFator, "Detalhamento dos Ganhadores por Pontuação");
                            if (exibirMaiorCaptura)
                                AdicionarDetalhamentoMembrosMaiorCaptura(col, membrosMaiorCaptura, todasCapturas, torneio, usarFator);
                        }
                    }

                });

                page.Footer().Element(footer => AdicionarRodapeRelatorio(footer, equipesRodape, patrocinadores));
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> GerarRelatorioMaioresCapturas(int quantidade)
    {
        if (quantidade < 1 || quantidade > 999)
            throw new InvalidOperationException("A quantidade deve estar entre 1 e 999.");

        var torneio = await _torneioServico.ObterPorId(_tenant.TorneioId);
        if (torneio is null)
            throw new InvalidOperationException("Dados nao encontrados para geracao do relatorio.");

        var capturas = (await _capturaServico.ListarTodos())
            .Where(c => !c.Invalidada)
            .OrderByDescending(c => c.TamanhoMedida)
            .ThenBy(c => c.DataHora)
            .Take(quantidade)
            .ToList();
        var patrocinadores = await ObterPatrocinadoresRelatorio();
        var equipesRodape = await ObterEquipesRelatorio(capturas.Select(c => c.EquipeId));

        var titulo = "Relatorio das Maiores Capturas";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => AdicionarCabecalhoRelatorio(
                    header,
                    torneio,
                    titulo,
                    $"Quantidade solicitada: {quantidade}"));

                page.Content().PaddingTop(12).Column(col =>
                {
                    if (!capturas.Any())
                    {
                        col.Item().Text("Nenhuma captura valida foi encontrada para o torneio.")
                            .FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    col.Item().Text($"Maiores capturas encontradas: {capturas.Count}").Bold();

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(30);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(3);
                            cols.ConstantColumn(65);
                            cols.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelItem).Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelMembro).Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelEquipe).Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"Medida ({torneio.MedidaCaptura})").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Data/Hora").Bold();
                        });

                        for (var i = 0; i < capturas.Count; i++)
                        {
                            var captura = capturas[i];
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                            table.Cell().Background(bg).Padding(4).Text((i + 1).ToString());
                            table.Cell().Background(bg).Padding(4).Text(captura.NomeItem);
                            table.Cell().Background(bg).Padding(4).Text(captura.NomeMembro);
                            table.Cell().Background(bg).Padding(4).Text(captura.NomeEquipe);
                            table.Cell().Background(bg).Padding(4).Text($"{captura.TamanhoMedida:F1}");
                            table.Cell().Background(bg).Padding(4).Text(captura.DataHora.ToString("dd/MM/yyyy HH:mm"));
                        }
                    });

                });

                page.Footer().Element(footer => AdicionarRodapeRelatorio(footer, equipesRodape, patrocinadores));
            });
        });

        return doc.GeneratePdf();
    }

    private void AdicionarFotos(ColumnDescriptor col, IEnumerable<CapturaDto> capturas)
    {
        foreach (var c in capturas.OrderBy(x => x.DataHora))
        {
            var fotoPath = string.IsNullOrWhiteSpace(c.FotoUrl)
                ? null
                : ResolverCaminhoFoto(c.FotoUrl);
            col.Item().PaddingTop(8).Row(row =>
            {
                row.AutoItem().Width(140).Column(inner =>
                {
                    if (fotoPath != null && File.Exists(fotoPath))
                    {
                        inner.Item().Image(fotoPath).FitArea();
                    }
                    else
                    {
                        inner.Item().Background(Colors.Grey.Lighten3).Width(130).Height(100)
                            .AlignCenter().AlignMiddle().Text("Foto indisponível").FontColor(Colors.Grey.Medium);
                    }
                });

                row.RelativeItem().PaddingLeft(8).Column(inner =>
                {
                    inner.Item().Text(c.NomeItem).Bold();
                    inner.Item().Text($"Membro: {c.NomeMembro}");
                    inner.Item().Text($"Medida: {c.TamanhoMedida:F1}");
                    inner.Item().Text($"Pontos: {c.Pontuacao:F2}");
                    inner.Item().Text($"Data/Hora: {c.DataHora:dd/MM/yyyy HH:mm}");
                });
            });
        }
    }

    private async Task<List<Application.DTOs.Patrocinador.PatrocinadorDto>> ObterPatrocinadoresRelatorio()
    {
        var lista = await _patrocinadorServico.ListarPorTorneio(_tenant.TorneioId);
        return lista
            .Where(p => p.ExibirNosRelatorios)
            .OrderBy(p => p.Nome)
            .ToList();
    }

    private async Task<List<EquipeDto>> ObterEquipesRelatorio(IEnumerable<Guid> equipeIds)
    {
        var ids = equipeIds.Distinct().ToHashSet();
        if (ids.Count == 0)
            return [];

        var equipes = await _equipeServico.ListarTodos();
        return equipes
            .Where(e => ids.Contains(e.Id) && !string.IsNullOrWhiteSpace(e.FotoUrl))
            .OrderBy(e => e.Nome)
            .ToList();
    }

    private void AdicionarCabecalhoRelatorio(
        IContainer container,
        TorneioDto torneio,
        string titulo,
        string? subtitulo = null)
    {
        var logoPath = string.IsNullOrWhiteSpace(torneio.LogoUrl)
            ? null
            : ResolverCaminhoFoto(torneio.LogoUrl);

        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (logoPath != null && File.Exists(logoPath))
                {
                    row.AutoItem().Width(64).Height(64).Image(logoPath).FitArea();
                    row.ConstantItem(12);
                }

                row.RelativeItem().Column(texto =>
                {
                    texto.Item().Text(torneio.NomeTorneio).Bold().FontSize(16);
                    texto.Item().Text(titulo).FontSize(12).Italic();
                    if (!string.IsNullOrWhiteSpace(subtitulo))
                        texto.Item().Text(subtitulo).FontSize(10);
                });
            });

            col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text(string.Empty);
        });
    }

    private void AdicionarRodapeRelatorio(
        IContainer container,
        IReadOnlyCollection<EquipeDto> equipes,
        IReadOnlyCollection<PatrocinadorDto> patrocinadores)
    {
        container.Column(col =>
        {
            if (equipes.Count > 0)
            {
                col.Item().Text("Embarcacoes").Bold().FontSize(9);
                col.Item().PaddingTop(4).Element(area => AdicionarGaleriaImagensRodape(
                    area,
                    equipes.Select(e => (e.Nome, (string?)e.FotoUrl)).ToList()));
            }

            if (patrocinadores.Count > 0)
            {
                col.Item().PaddingTop(equipes.Count > 0 ? 6 : 0).Text("Patrocinadores").Bold().FontSize(9);
                col.Item().PaddingTop(4).Element(area => AdicionarGaleriaImagensRodape(
                    area,
                    patrocinadores.Select(p => (p.Nome, (string?)p.FotoUrl)).ToList()));
            }

            col.Item().PaddingTop(6).AlignCenter().Text(x =>
            {
                x.Span("Pagina ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        });
    }

    private void AdicionarGaleriaImagensRodape(
        IContainer container,
        IReadOnlyCollection<(string Nome, string? FotoUrl)> imagens)
    {
        var imagensValidas = imagens
            .Select(x => (x.Nome, Caminho: string.IsNullOrWhiteSpace(x.FotoUrl) ? null : ResolverCaminhoFoto(x.FotoUrl)))
            .Where(x => x.Caminho != null && File.Exists(x.Caminho))
            .ToList();

        if (imagensValidas.Count == 0)
            return;

        container.Table(table =>
        {
            const int colunas = 6;
            table.ColumnsDefinition(cols =>
            {
                for (var i = 0; i < colunas; i++)
                    cols.RelativeColumn();
            });

            var indice = 0;
            foreach (var imagem in imagensValidas)
            {
                table.Cell().Padding(2).Column(inner =>
                {
                    inner.Item().Height(36).AlignCenter().Image(imagem.Caminho!).FitArea();
                    inner.Item().AlignCenter().Text(imagem.Nome).FontSize(7);
                });

                indice++;
                if (indice % colunas != 0)
                    continue;
            }

            var faltantes = (colunas - (imagensValidas.Count % colunas)) % colunas;
            for (var i = 0; i < faltantes; i++)
                table.Cell().Padding(2).Text(string.Empty);
        });
    }

    private void AdicionarPatrocinadores(
        ColumnDescriptor col,
        IReadOnlyCollection<Application.DTOs.Patrocinador.PatrocinadorDto> patrocinadores)
    {
        if (patrocinadores.Count == 0)
            return;

        col.Item().PaddingTop(18).Text("Patrocinadores").Bold().FontSize(12);
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(70);
                cols.RelativeColumn(3);
                cols.RelativeColumn(4);
            });

            var index = 0;
            foreach (var patrocinador in patrocinadores)
            {
                var bg = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                var fotoPath = ResolverCaminhoFoto(patrocinador.FotoUrl);
                table.Cell().Background(bg).Padding(6).Element(cell =>
                {
                    if (fotoPath != null && File.Exists(fotoPath))
                    {
                        cell.Height(52).Image(fotoPath).FitArea();
                    }
                    else
                    {
                        cell.Height(52).AlignCenter().AlignMiddle().Text("Sem imagem").FontSize(8).FontColor(Colors.Grey.Medium);
                    }
                });
                table.Cell().Background(bg).Padding(6).Text(patrocinador.Nome).Bold();
                table.Cell().Background(bg).Padding(6).Column(inner =>
                {
                    if (!string.IsNullOrWhiteSpace(patrocinador.Site))
                        inner.Item().Text($"Site: {patrocinador.Site}");
                    if (!string.IsNullOrWhiteSpace(patrocinador.Instagram))
                        inner.Item().Text($"Instagram: {patrocinador.Instagram}");
                    if (!string.IsNullOrWhiteSpace(patrocinador.Facebook))
                        inner.Item().Text($"Facebook: {patrocinador.Facebook}");
                    if (!string.IsNullOrWhiteSpace(patrocinador.Zap))
                        inner.Item().Text($"Zap: {patrocinador.Zap}");
                });
                index++;
            }
        });
    }

    private string? ResolverCaminhoFoto(string fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(fotoUrl)) return null;

        if (Path.IsPathRooted(fotoUrl)) return fotoUrl;

        if (!string.IsNullOrWhiteSpace(_storage.BasePath))
        {
            var relative = fotoUrl.TrimStart('/', '\\');
            return Path.Combine(_storage.BasePath, relative);
        }

        return null;
    }

    private static void AdicionarResumoEquipes(
        ColumnDescriptor col,
        IReadOnlyCollection<ResumoEquipeGanhadora> equipes,
        TorneioDto torneio,
        bool exibirPescadoresDasEmbarcacoes)
    {
        if (equipes.Count == 0)
            return;

        col.Item().Text($"Ranking de {torneio.LabelEquipePlural} Ganhadoras").Bold().FontSize(12);
        col.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(36);
                cols.RelativeColumn(3);
                cols.RelativeColumn(3);
                if (exibirPescadoresDasEmbarcacoes) cols.RelativeColumn(4);
                cols.ConstantColumn(65);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelEquipe).Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Capitão").Bold();
                if (exibirPescadoresDasEmbarcacoes) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelMembroPlural).Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Pontos").Bold();
            });

            foreach (var equipe in equipes)
            {
                var bg = equipe.Posicao % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                table.Cell().Background(bg).Padding(4).Text(equipe.Posicao.ToString());
                table.Cell().Background(bg).Padding(4).Text(equipe.NomeEquipe);
                table.Cell().Background(bg).Padding(4).Text(equipe.Capitao);
                if (exibirPescadoresDasEmbarcacoes) table.Cell().Background(bg).Padding(4).Text(string.Join(", ", equipe.Pescadores));
                table.Cell().Background(bg).Padding(4).Text($"{equipe.TotalPontos:F2}");
            }
        });
        col.Item().PaddingTop(12).Text(string.Empty);
    }

    private static void AdicionarResumoMembrosPontuacao(ColumnDescriptor col, IReadOnlyCollection<ResumoMembroGanhador> membros, TorneioDto torneio)
    {
        if (membros.Count == 0)
            return;

        col.Item().Text($"Ranking de {torneio.LabelMembroPlural} por Pontuacao").Bold().FontSize(12);
        col.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(36);
                cols.RelativeColumn(4);
                cols.ConstantColumn(65);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelMembro).Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Pontos").Bold();
            });

            foreach (var membro in membros)
            {
                var bg = membro.Posicao % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                table.Cell().Background(bg).Padding(4).Text(membro.Posicao.ToString());
                table.Cell().Background(bg).Padding(4).Text(membro.NomeMembro);
                table.Cell().Background(bg).Padding(4).Text($"{membro.TotalPontos:F2}");
            }
        });
        col.Item().PaddingTop(12).Text(string.Empty);
    }

    private static void AdicionarResumoMembrosMaiorCaptura(ColumnDescriptor col, IReadOnlyCollection<ResumoMembroGanhador> membros, TorneioDto torneio)
    {
        if (membros.Count == 0)
            return;

        col.Item().Text($"Ranking de {torneio.LabelMembroPlural} por Maior Captura").Bold().FontSize(12);
        col.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(36);
                cols.RelativeColumn(4);
                cols.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelMembro).Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"Maior ({torneio.MedidaCaptura})").Bold();
            });

            foreach (var membro in membros)
            {
                var bg = membro.Posicao % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                var descricao = $"{(membro.MaiorCaptura ?? 0m):F1}";
                if (!string.IsNullOrWhiteSpace(membro.NomeItemMaiorCaptura))
                    descricao += $" - {membro.NomeItemMaiorCaptura}";
                table.Cell().Background(bg).Padding(4).Text(membro.Posicao.ToString());
                table.Cell().Background(bg).Padding(4).Text(membro.NomeMembro);
                table.Cell().Background(bg).Padding(4).Text(descricao);
            }
        });
        col.Item().PaddingTop(12).Text(string.Empty);
    }

    private static void AdicionarDetalhamentoEquipes(
        ColumnDescriptor col,
        IReadOnlyCollection<ResumoEquipeGanhadora> equipes,
        IReadOnlyCollection<CapturaDto> capturas,
        TorneioDto torneio,
        bool usarFator,
        bool exibirPescadoresDasEmbarcacoes)
    {
        if (equipes.Count == 0)
            return;

        col.Item().PaddingTop(8).Text("Detalhamento das Embarcações Ganhadoras").Bold().FontSize(12);
        foreach (var equipe in equipes)
        {
            var capturasEquipe = capturas
                .Where(c => c.EquipeId == equipe.EquipeId)
                .OrderBy(c => c.NomeMembro)
                .ThenBy(c => c.DataHora)
                .ToList();

            col.Item().PaddingTop(10).Text($"{equipe.Posicao}o lugar - {equipe.NomeEquipe}").Bold();
            col.Item().Text($"Capitão: {equipe.Capitao}");
            if (exibirPescadoresDasEmbarcacoes && equipe.Pescadores.Count > 0)
                col.Item().Text($"{torneio.LabelMembroPlural}: {string.Join(", ", equipe.Pescadores)}");
            col.Item().Text($"Pontos: {equipe.TotalPontos:F2}");
            AdicionarTabelaCapturas(col, capturasEquipe, torneio, usarFator, exibirEquipe: false, exibirMembro: true);
        }
    }

    private static void AdicionarDetalhamentoMembros(
        ColumnDescriptor col,
        IReadOnlyCollection<ResumoMembroGanhador> membros,
        IReadOnlyCollection<CapturaDto> capturas,
        TorneioDto torneio,
        bool usarFator,
        string titulo)
    {
        if (membros.Count == 0)
            return;

        col.Item().PaddingTop(8).Text(titulo).Bold().FontSize(12);
        foreach (var membro in membros)
        {
            var capturasMembro = capturas
                .Where(c => c.MembroId == membro.MembroId)
                .OrderBy(c => c.DataHora)
                .ToList();

            col.Item().PaddingTop(10).Text($"{membro.Posicao}o lugar - {membro.NomeMembro}").Bold();
            col.Item().Text($"Pontos: {membro.TotalPontos:F2}");
            AdicionarTabelaCapturas(col, capturasMembro, torneio, usarFator, exibirEquipe: true, exibirMembro: false);
        }
    }

    private static void AdicionarDetalhamentoMembrosMaiorCaptura(
        ColumnDescriptor col,
        IReadOnlyCollection<ResumoMembroGanhador> membros,
        IReadOnlyCollection<CapturaDto> capturas,
        TorneioDto torneio,
        bool usarFator)
    {
        if (membros.Count == 0)
            return;

        col.Item().PaddingTop(8).Text("Detalhamento dos Ganhadores por Maior Captura").Bold().FontSize(12);
        foreach (var membro in membros)
        {
            var capturasMembro = capturas
                .Where(c => c.MembroId == membro.MembroId)
                .OrderByDescending(c => c.TamanhoMedida)
                .ThenBy(c => c.DataHora)
                .ToList();

            col.Item().PaddingTop(10).Text($"{membro.Posicao}o lugar - {membro.NomeMembro}").Bold();
            col.Item().Text($"Maior captura: {(membro.MaiorCaptura ?? 0m):F1} {torneio.MedidaCaptura}" +
                            (!string.IsNullOrWhiteSpace(membro.NomeItemMaiorCaptura) ? $" - {membro.NomeItemMaiorCaptura}" : string.Empty));
            AdicionarTabelaCapturas(col, capturasMembro, torneio, usarFator, exibirEquipe: true, exibirMembro: false);
        }
    }

    private static void AdicionarTabelaCapturas(
        ColumnDescriptor col,
        IReadOnlyList<CapturaDto> capturas,
        TorneioDto torneio,
        bool usarFator,
        bool exibirEquipe,
        bool exibirMembro)
    {
        if (capturas.Count == 0)
        {
            col.Item().Text("Nenhuma captura valida encontrada para este registro.").FontColor(Colors.Grey.Darken1);
            return;
        }

        col.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(26);
                cols.RelativeColumn(3);
                if (exibirMembro) cols.RelativeColumn(3);
                if (exibirEquipe) cols.RelativeColumn(3);
                cols.ConstantColumn(58);
                if (usarFator) cols.ConstantColumn(46);
                cols.ConstantColumn(54);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelItem).Bold();
                if (exibirMembro) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelMembro).Bold();
                if (exibirEquipe) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(torneio.LabelEquipe).Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"Medida ({torneio.MedidaCaptura})").Bold();
                if (usarFator) header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Fator").Bold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Pontos").Bold();
            });

            for (var i = 0; i < capturas.Count; i++)
            {
                var captura = capturas[i];
                var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                table.Cell().Background(bg).Padding(4).Text((i + 1).ToString());
                table.Cell().Background(bg).Padding(4).Text(captura.NomeItem);
                if (exibirMembro) table.Cell().Background(bg).Padding(4).Text(captura.NomeMembro);
                if (exibirEquipe) table.Cell().Background(bg).Padding(4).Text(captura.NomeEquipe);
                table.Cell().Background(bg).Padding(4).Text($"{captura.TamanhoMedida:F1}");
                if (usarFator) table.Cell().Background(bg).Padding(4).Text(captura.FatorMultiplicador > 1m ? captura.FatorMultiplicador.ToString("F2") : "-");
                table.Cell().Background(bg).Padding(4).Text($"{captura.Pontuacao:F2}");
            }
        });
    }

    private sealed class ResumoEquipeGanhadora
    {
        public int Posicao { get; set; }
        public Guid EquipeId { get; set; }
        public string NomeEquipe { get; set; } = string.Empty;
        public string Capitao { get; set; } = string.Empty;
        public List<string> Pescadores { get; set; } = [];
        public decimal TotalPontos { get; set; }
        public DateTime PrimeiraCaptura { get; set; }
    }

    private sealed class ResumoMembroGanhador
    {
        public int Posicao { get; set; }
        public Guid MembroId { get; set; }
        public string NomeMembro { get; set; } = string.Empty;
        public decimal TotalPontos { get; set; }
        public decimal? MaiorCaptura { get; set; }
        public string? NomeItemMaiorCaptura { get; set; }
        public DateTime PrimeiraCaptura { get; set; }
    }
}
