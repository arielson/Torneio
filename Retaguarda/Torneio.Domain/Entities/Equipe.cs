using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class Equipe
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }
    public string Capitao { get; private set; } = null!;
    public string? FotoCapitaoUrl { get; private set; }
    public int QtdVagas { get; private set; }
    public decimal Custo { get; private set; }
    public StatusEmbarcacaoFinanceira StatusFinanceiro { get; private set; }

    private readonly List<Membro> _membros = new();
    private readonly List<FiscalEquipe> _fiscais = new();

    public IReadOnlyCollection<Membro> Membros => _membros.AsReadOnly();
    public IReadOnlyCollection<FiscalEquipe> Fiscais => _fiscais.AsReadOnly();

    private Equipe() { }

    public static Equipe Criar(
        Guid torneioId,
        string nome,
        string capitao,
        int qtdVagas,
        string? fotoUrl = null,
        string? fotoCapitaoUrl = null,
        decimal custo = 0,
        StatusEmbarcacaoFinanceira statusFinanceiro = StatusEmbarcacaoFinanceira.Pendente)
    {
        return new Equipe
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            Capitao = capitao,
            QtdVagas = qtdVagas,
            FotoUrl = fotoUrl,
            FotoCapitaoUrl = fotoCapitaoUrl,
            Custo = custo,
            StatusFinanceiro = statusFinanceiro
        };
    }

    public void AdicionarMembro(Membro membro)
    {
        if (_membros.Count >= QtdVagas)
            throw new InvalidOperationException("A equipe ja atingiu o numero maximo de vagas.");
        if (_membros.Any(m => m.Id == membro.Id))
            throw new InvalidOperationException("O membro ja pertence a esta equipe.");
        _membros.Add(membro);
    }

    public void RemoverMembro(Guid membroId)
    {
        var membro = _membros.FirstOrDefault(m => m.Id == membroId);
        if (membro is null)
            throw new InvalidOperationException("Membro nao encontrado na equipe.");
        _membros.Remove(membro);
    }

    public void Atualizar(
        string nome,
        string capitao,
        int qtdVagas,
        string? fotoUrl = null,
        string? fotoCapitaoUrl = null,
        decimal? custo = null,
        StatusEmbarcacaoFinanceira? statusFinanceiro = null)
    {
        Nome = nome;
        Capitao = capitao;
        QtdVagas = qtdVagas;
        if (fotoUrl != null) FotoUrl = fotoUrl;
        if (fotoCapitaoUrl != null) FotoCapitaoUrl = fotoCapitaoUrl;
        if (custo.HasValue) Custo = custo.Value;
        if (statusFinanceiro.HasValue) StatusFinanceiro = statusFinanceiro.Value;
    }
}
