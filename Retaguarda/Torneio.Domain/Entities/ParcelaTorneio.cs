using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class ParcelaTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid MembroId { get; private set; }
    public TipoParcelaTorneio TipoParcela { get; private set; }
    public int NumeroParcela { get; private set; }
    public Guid? ReferenciaId { get; private set; }
    public string Descricao { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public DateTime Vencimento { get; private set; }
    public bool VencimentoEditadoManual { get; private set; }
    public bool Pago { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public string? Observacao { get; private set; }
    public string? ComprovanteNomeArquivo { get; private set; }
    public DateTime? ComprovanteDataUpload { get; private set; }
    public string? ComprovanteUsuarioNome { get; private set; }
    public string? ComprovanteUrl { get; private set; }
    public string? ComprovanteContentType { get; private set; }

    private ParcelaTorneio() { }

    public static ParcelaTorneio Criar(
        Guid torneioId,
        Guid membroId,
        TipoParcelaTorneio tipoParcela,
        int numeroParcela,
        string descricao,
        decimal valor,
        DateTime vencimento,
        Guid? referenciaId = null)
    {
        return new ParcelaTorneio
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            MembroId = membroId,
            TipoParcela = tipoParcela,
            NumeroParcela = numeroParcela,
            ReferenciaId = referenciaId,
            Descricao = descricao.Trim(),
            Valor = valor,
            Vencimento = vencimento,
            VencimentoEditadoManual = false,
            Pago = false
        };
    }

    public void AtualizarValor(decimal valor)
    {
        Valor = valor;
    }

    public void AtualizarDescricao(string descricao)
    {
        Descricao = descricao.Trim();
    }

    public void AtualizarVencimento(DateTime vencimento, bool editadoManual = true)
    {
        Vencimento = vencimento;
        VencimentoEditadoManual = editadoManual;
    }

    public void AtualizarObservacao(string? observacao)
    {
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }

    public void MarcarComoPago(DateTime? dataPagamento = null)
    {
        Pago = true;
        DataPagamento ??= dataPagamento ?? DateTime.UtcNow;
    }

    public void DesmarcarPagamento()
    {
        Pago = false;
        DataPagamento = null;
    }

    public void AtualizarComprovante(
        string nomeArquivo,
        string url,
        string? contentType,
        string usuarioNome,
        DateTime? dataUpload = null)
    {
        ComprovanteNomeArquivo = nomeArquivo;
        ComprovanteUrl = url;
        ComprovanteContentType = contentType;
        ComprovanteUsuarioNome = usuarioNome;
        ComprovanteDataUpload = dataUpload ?? DateTime.UtcNow;
    }
}
