using Torneio.Domain.Enums;

namespace Torneio.Application.Common;

/// <summary>
/// Configurações pré-definidas por tipo de torneio.
/// Usado para preencher automaticamente os campos ao criar um torneio.
/// </summary>
public static class TorneioPresets
{
    public record Preset(
        string LabelEquipe,
        string LabelMembro,
        string LabelSupervisor,
        string LabelItem,
        string LabelCaptura,
        string MedidaCaptura,
        bool UsarFatorMultiplicador,
        ModoSorteio ModoSorteio);

    public static readonly Dictionary<TipoTorneio, Preset> Todos = new()
    {
        [TipoTorneio.Pesca] = new Preset(
            LabelEquipe: "Embarcação",
            LabelMembro: "Pescador",
            LabelSupervisor: "Fiscal",
            LabelItem: "Peixe",
            LabelCaptura: "Captura",
            MedidaCaptura: "cm",
            UsarFatorMultiplicador: false,
            ModoSorteio: ModoSorteio.Sorteio),

        [TipoTorneio.Futebol] = new Preset(
            LabelEquipe: "Time",
            LabelMembro: "Jogador",
            LabelSupervisor: "Árbitro",
            LabelItem: "Categoria",
            LabelCaptura: "Gol",
            MedidaCaptura: "gol",
            UsarFatorMultiplicador: false,
            ModoSorteio: ModoSorteio.Sorteio),
    };

    public static Preset Get(TipoTorneio tipo) =>
        Todos.TryGetValue(tipo, out var preset) ? preset : Todos[TipoTorneio.Pesca];
}
