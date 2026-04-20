namespace Torneio.Domain.Entities;

/// <summary>
/// Equipe pre-formada para o modo GrupoEquipe.
/// No sorteio, cada Grupo sorteia uma Equipe (embarcação).
/// </summary>
public class Grupo
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;

    private readonly List<GrupoMembro> _membros = new();
    public IReadOnlyCollection<GrupoMembro> Membros => _membros.AsReadOnly();

    private Grupo() { }

    public static Grupo Criar(Guid torneioId, string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do grupo é obrigatório.", nameof(nome));

        return new Grupo
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome.Trim()
        };
    }

    public void Renomear(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new ArgumentException("O nome do grupo é obrigatório.", nameof(novoNome));
        Nome = novoNome.Trim();
    }

    public void AdicionarMembro(GrupoMembro membro)
    {
        if (_membros.Any(m => m.MembroId == membro.MembroId))
            throw new InvalidOperationException("O membro já está neste grupo.");
        _membros.Add(membro);
    }

    public void RemoverMembro(Guid grupoMembroId)
    {
        var item = _membros.FirstOrDefault(m => m.Id == grupoMembroId)
            ?? throw new InvalidOperationException("Membro não encontrado no grupo.");
        _membros.Remove(item);
    }
}
