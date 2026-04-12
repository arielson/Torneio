using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class TorneioEntity
{
    public Guid Id { get; private set; }
    public string Slug { get; private set; } = null!;
    public string NomeTorneio { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public bool Ativo { get; private set; }

    // Terminologia configurável
    public string LabelEquipe { get; private set; } = null!;
    public string LabelMembro { get; private set; } = null!;
    public string LabelSupervisor { get; private set; } = null!;
    public string LabelItem { get; private set; } = null!;
    public string LabelCaptura { get; private set; } = null!;

    // Regras
    public bool UsarFatorMultiplicador { get; private set; }
    public string MedidaCaptura { get; private set; } = null!;
    public bool PermitirCapturaOffline { get; private set; }

    // Sorteio
    public ModoSorteio ModoSorteio { get; private set; }

    // Tipo (define terminologia fixa)
    public TipoTorneio TipoTorneio { get; private set; } = TipoTorneio.Pesca;

    private TorneioEntity() { }

    public static TorneioEntity Criar(
        string slug,
        string nomeTorneio,
        string labelEquipe,
        string labelMembro,
        string labelSupervisor,
        string labelItem,
        string labelCaptura,
        string medidaCaptura,
        ModoSorteio modoSorteio,
        TipoTorneio tipoTorneio = TipoTorneio.Pesca,
        bool usarFatorMultiplicador = false,
        bool permitirCapturaOffline = true,
        string? logoUrl = null)
    {
        return new TorneioEntity
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            NomeTorneio = nomeTorneio,
            LogoUrl = logoUrl,
            Ativo = true,
            LabelEquipe = labelEquipe,
            LabelMembro = labelMembro,
            LabelSupervisor = labelSupervisor,
            LabelItem = labelItem,
            LabelCaptura = labelCaptura,
            MedidaCaptura = medidaCaptura,
            UsarFatorMultiplicador = usarFatorMultiplicador,
            PermitirCapturaOffline = permitirCapturaOffline,
            ModoSorteio = modoSorteio,
            TipoTorneio = tipoTorneio,
        };
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public void AtualizarConfiguracoes(
        string nomeTorneio,
        string labelEquipe,
        string labelMembro,
        string labelSupervisor,
        string labelItem,
        string labelCaptura,
        string medidaCaptura,
        ModoSorteio modoSorteio,
        bool usarFatorMultiplicador,
        bool permitirCapturaOffline,
        string? logoUrl = null)
    {
        NomeTorneio = nomeTorneio;
        LabelEquipe = labelEquipe;
        LabelMembro = labelMembro;
        LabelSupervisor = labelSupervisor;
        LabelItem = labelItem;
        LabelCaptura = labelCaptura;
        MedidaCaptura = medidaCaptura;
        ModoSorteio = modoSorteio;
        UsarFatorMultiplicador = usarFatorMultiplicador;
        PermitirCapturaOffline = permitirCapturaOffline;
        if (logoUrl != null) LogoUrl = logoUrl;
    }
}
