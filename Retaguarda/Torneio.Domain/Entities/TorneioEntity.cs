using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class TorneioEntity
{
    public Guid Id { get; private set; }
    public string Slug { get; private set; } = null!;
    public string NomeTorneio { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public bool Ativo { get; private set; }

    // Status do torneio
    public StatusTorneio Status { get; private set; }

    // Terminologia configurável — singular e plural
    public string LabelEquipe { get; private set; } = null!;
    public string LabelEquipePlural { get; private set; } = null!;
    public string LabelMembro { get; private set; } = null!;
    public string LabelMembroPlural { get; private set; } = null!;
    public string LabelSupervisor { get; private set; } = null!;
    public string LabelSupervisorPlural { get; private set; } = null!;
    public string LabelItem { get; private set; } = null!;
    public string LabelItemPlural { get; private set; } = null!;
    public string LabelCaptura { get; private set; } = null!;
    public string LabelCapturaPlural { get; private set; } = null!;

    // Regras
    public bool UsarFatorMultiplicador { get; private set; }
    public string MedidaCaptura { get; private set; } = null!;
    public bool PermitirCapturaOffline { get; private set; }

    // Sorteio
    public ModoSorteio ModoSorteio { get; private set; }

    // Premiação — quantos lugares premiados (1º, 2º, 3º…)
    public int QtdGanhadores { get; private set; } = 3;

    // Premiação — a quem se destina o ranking de ganhadores
    public bool PremiacaoPorEquipe { get; private set; } = true;
    public bool PremiacaoPorMembro { get; private set; } = false;

    // Tipo (define terminologia fixa)
    public TipoTorneio TipoTorneio { get; private set; } = TipoTorneio.Pesca;

    // Tema visual (cor primária em hex, ex: "#1976D2"). null = usa o padrão da plataforma.
    public string? CorPrimaria { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private TorneioEntity() { }

    public static TorneioEntity Criar(
        string slug,
        string nomeTorneio,
        string labelEquipe,
        string labelEquipePlural,
        string labelMembro,
        string labelMembroPlural,
        string labelSupervisor,
        string labelSupervisorPlural,
        string labelItem,
        string labelItemPlural,
        string labelCaptura,
        string labelCapturaPlural,
        string medidaCaptura,
        ModoSorteio modoSorteio,
        TipoTorneio tipoTorneio = TipoTorneio.Pesca,
        bool usarFatorMultiplicador = false,
        bool permitirCapturaOffline = true,
        int qtdGanhadores = 3,
        bool premiacaoPorEquipe = true,
        bool premiacaoPorMembro = false,
        string? logoUrl = null,
        string? corPrimaria = null)
    {
        return new TorneioEntity
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            NomeTorneio = nomeTorneio,
            LogoUrl = logoUrl,
            Ativo = true,
            Status = StatusTorneio.Aberto,
            LabelEquipe = labelEquipe,
            LabelEquipePlural = labelEquipePlural,
            LabelMembro = labelMembro,
            LabelMembroPlural = labelMembroPlural,
            LabelSupervisor = labelSupervisor,
            LabelSupervisorPlural = labelSupervisorPlural,
            LabelItem = labelItem,
            LabelItemPlural = labelItemPlural,
            LabelCaptura = labelCaptura,
            LabelCapturaPlural = labelCapturaPlural,
            MedidaCaptura = medidaCaptura,
            UsarFatorMultiplicador = usarFatorMultiplicador,
            PermitirCapturaOffline = permitirCapturaOffline,
            ModoSorteio = modoSorteio,
            TipoTorneio = tipoTorneio,
            QtdGanhadores = qtdGanhadores,
            PremiacaoPorEquipe = premiacaoPorEquipe,
            PremiacaoPorMembro = premiacaoPorMembro,
            CorPrimaria = corPrimaria,
            CriadoEm = DateTime.UtcNow,
        };
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public void Liberar()
    {
        if (Status != StatusTorneio.Aberto)
            throw new InvalidOperationException("Somente torneios com status Aberto podem ser liberados.");
        Status = StatusTorneio.Liberado;
    }

    public void Finalizar()
    {
        if (Status != StatusTorneio.Liberado)
            throw new InvalidOperationException("Somente torneios com status Liberado podem ser finalizados.");
        Status = StatusTorneio.Finalizado;
    }

    public void Reabrir()
    {
        if (Status == StatusTorneio.Aberto)
            throw new InvalidOperationException("O torneio já está aberto.");
        Status = StatusTorneio.Aberto;
    }

    public void AtualizarConfiguracoes(
        string nomeTorneio,
        string labelEquipe,
        string labelEquipePlural,
        string labelMembro,
        string labelMembroPlural,
        string labelSupervisor,
        string labelSupervisorPlural,
        string labelItem,
        string labelItemPlural,
        string labelCaptura,
        string labelCapturaPlural,
        string medidaCaptura,
        ModoSorteio modoSorteio,
        bool usarFatorMultiplicador,
        bool permitirCapturaOffline,
        int qtdGanhadores,
        bool premiacaoPorEquipe = true,
        bool premiacaoPorMembro = false,
        string? logoUrl = null,
        string? corPrimaria = null)
    {
        NomeTorneio = nomeTorneio;
        LabelEquipe = labelEquipe;
        LabelEquipePlural = labelEquipePlural;
        LabelMembro = labelMembro;
        LabelMembroPlural = labelMembroPlural;
        LabelSupervisor = labelSupervisor;
        LabelSupervisorPlural = labelSupervisorPlural;
        LabelItem = labelItem;
        LabelItemPlural = labelItemPlural;
        LabelCaptura = labelCaptura;
        LabelCapturaPlural = labelCapturaPlural;
        MedidaCaptura = medidaCaptura;
        ModoSorteio = modoSorteio;
        UsarFatorMultiplicador = usarFatorMultiplicador;
        PermitirCapturaOffline = permitirCapturaOffline;
        QtdGanhadores = qtdGanhadores;
        PremiacaoPorEquipe = premiacaoPorEquipe;
        PremiacaoPorMembro = premiacaoPorMembro;
        if (logoUrl != null) LogoUrl = logoUrl;
        CorPrimaria = corPrimaria;
    }
}
