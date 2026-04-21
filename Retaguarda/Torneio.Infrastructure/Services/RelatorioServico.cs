using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.Services.Interfaces;
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

                page.Header().Column(col =>
                {
                    col.Item().Text(torneio.NomeTorneio).Bold().FontSize(16);
                    col.Item().Text(titulo).FontSize(12).Italic();
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("");
                });

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

                    AdicionarPatrocinadores(col, patrocinadores);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
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

                page.Header().Column(col =>
                {
                    col.Item().Text(torneio.NomeTorneio).Bold().FontSize(16);
                    col.Item().Text(titulo).FontSize(12).Italic();
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("");
                });

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

                    AdicionarPatrocinadores(col, patrocinadores);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
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

        var titulo = "Relatorio das Maiores Capturas";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(torneio.NomeTorneio).Bold().FontSize(16);
                    col.Item().Text(titulo).FontSize(12).Italic();
                    col.Item().Text($"Quantidade solicitada: {quantidade}").FontSize(10);
                    col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("");
                });

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

                    AdicionarPatrocinadores(col, patrocinadores);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    private void AdicionarFotos(ColumnDescriptor col, IEnumerable<CapturaDto> capturas)
    {
        foreach (var c in capturas.OrderBy(x => x.DataHora))
        {
            var fotoPath = ResolverCaminhoFoto(c.FotoUrl);
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
}
