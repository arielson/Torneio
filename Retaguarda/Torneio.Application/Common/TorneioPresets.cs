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
        string LabelEquipePlural,
        string LabelMembro,
        string LabelMembroPlural,
        string LabelSupervisor,
        string LabelSupervisorPlural,
        string LabelItem,
        string LabelItemPlural,
        string LabelCaptura,
        string LabelCapturaPlural,
        string MedidaCaptura,
        bool UsarFatorMultiplicador,
        ModoSorteio ModoSorteio);

    public static readonly Dictionary<TipoTorneio, Preset> Todos = new()
    {
        [TipoTorneio.Pesca] = new Preset(
            LabelEquipe: "Embarcação",
            LabelEquipePlural: "Embarcações",
            LabelMembro: "Pescador",
            LabelMembroPlural: "Pescadores",
            LabelSupervisor: "Fiscal",
            LabelSupervisorPlural: "Fiscais",
            LabelItem: "Peixe",
            LabelItemPlural: "Peixes",
            LabelCaptura: "Captura",
            LabelCapturaPlural: "Capturas",
            MedidaCaptura: "cm",
            UsarFatorMultiplicador: false,
            ModoSorteio: ModoSorteio.Nenhum),
    };

    public static Preset Get(TipoTorneio tipo) =>
        Todos.TryGetValue(tipo, out var preset) ? preset : Todos[TipoTorneio.Pesca];
}
