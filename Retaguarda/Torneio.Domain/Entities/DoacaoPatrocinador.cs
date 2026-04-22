using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class DoacaoPatrocinador
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid? PatrocinadorId { get; private set; }
    public string NomePatrocinador { get; private set; } = null!;
    public TipoDoacaoPatrocinador Tipo { get; private set; }
    public string Descricao { get; private set; } = null!;
    public decimal? Quantidade { get; private set; }
    public decimal? Valor { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime DataDoacao { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private DoacaoPatrocinador() { }

    public static DoacaoPatrocinador Criar(
        Guid torneioId,
        Guid? patrocinadorId,
        string nomePatrocinador,
        TipoDoacaoPatrocinador tipo,
        string descricao,
        decimal? quantidade,
        decimal? valor,
        string? observacao,
        DateTime dataDoacao)
    {
        var entidade = new DoacaoPatrocinador
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            CriadoEm = DateTime.UtcNow
        };
        entidade.Atualizar(patrocinadorId, nomePatrocinador, tipo, descricao, quantidade, valor, observacao, dataDoacao);
        return entidade;
    }

    public void Atualizar(
        Guid? patrocinadorId,
        string nomePatrocinador,
        TipoDoacaoPatrocinador tipo,
        string descricao,
        decimal? quantidade,
        decimal? valor,
        string? observacao,
        DateTime dataDoacao)
    {
        PatrocinadorId = patrocinadorId;
        NomePatrocinador = nomePatrocinador.Trim();
        Tipo = tipo;
        Descricao = descricao.Trim();
        Quantidade = quantidade;
        Valor = valor;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        DataDoacao = dataDoacao.Date;
    }
}
