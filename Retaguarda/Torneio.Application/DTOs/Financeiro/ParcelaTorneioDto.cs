namespace Torneio.Application.DTOs.Financeiro;

public class ParcelaTorneioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid MembroId { get; init; }
    public string NomeMembro { get; init; } = null!;
    public string TipoParcela { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public int NumeroParcela { get; init; }
    public decimal Valor { get; init; }
    public DateTime Vencimento { get; init; }
    public bool VencimentoEditadoManual { get; init; }
    public bool Pago { get; init; }
    public DateTime? DataPagamento { get; init; }
    public string? Observacao { get; init; }
    public bool Inadimplente { get; init; }
    public string? ComprovanteNomeArquivo { get; init; }
    public DateTime? ComprovanteDataUpload { get; init; }
    public string? ComprovanteUsuarioNome { get; init; }
    public string? ComprovanteUrl { get; init; }
    public string? ComprovanteContentType { get; init; }
    public bool Bonificada { get; init; }
    public Guid? DoacaoPatrocinadorId { get; init; }
    public string? MotivoBonificacao { get; init; }
}
