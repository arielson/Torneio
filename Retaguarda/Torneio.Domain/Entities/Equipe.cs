namespace Torneio.Domain.Entities;

public class Equipe
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid AnoTorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }
    public string Capitao { get; private set; } = null!;
    public string? FotoCapitaoUrl { get; private set; }
    public Guid FiscalId { get; private set; }
    public int QtdVagas { get; private set; }

    private readonly List<Membro> _membros = new();
    public IReadOnlyCollection<Membro> Membros => _membros.AsReadOnly();

    private Equipe() { }

    public static Equipe Criar(
        Guid torneioId,
        Guid anoTorneioId,
        string nome,
        string capitao,
        Guid fiscalId,
        int qtdVagas,
        string? fotoUrl = null,
        string? fotoCapitaoUrl = null)
    {
        return new Equipe
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            AnoTorneioId = anoTorneioId,
            Nome = nome,
            Capitao = capitao,
            FiscalId = fiscalId,
            QtdVagas = qtdVagas,
            FotoUrl = fotoUrl,
            FotoCapitaoUrl = fotoCapitaoUrl
        };
    }

    public void AdicionarMembro(Membro membro)
    {
        if (_membros.Count >= QtdVagas)
            throw new InvalidOperationException("A equipe já atingiu o número máximo de vagas.");
        if (_membros.Any(m => m.Id == membro.Id))
            throw new InvalidOperationException("O membro já pertence a esta equipe.");
        _membros.Add(membro);
    }

    public void RemoverMembro(Guid membroId)
    {
        var membro = _membros.FirstOrDefault(m => m.Id == membroId);
        if (membro is null)
            throw new InvalidOperationException("Membro não encontrado na equipe.");
        _membros.Remove(membro);
    }

    public void Atualizar(string nome, string capitao, int qtdVagas, string? fotoUrl = null, string? fotoCapitaoUrl = null)
    {
        Nome = nome;
        Capitao = capitao;
        QtdVagas = qtdVagas;
        if (fotoUrl != null) FotoUrl = fotoUrl;
        if (fotoCapitaoUrl != null) FotoCapitaoUrl = fotoCapitaoUrl;
    }

    public void AtribuirFiscal(Guid fiscalId) => FiscalId = fiscalId;
}
