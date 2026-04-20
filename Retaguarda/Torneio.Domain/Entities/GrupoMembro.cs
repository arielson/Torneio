namespace Torneio.Domain.Entities;

/// <summary>
/// Associação entre um Grupo e um Membro (participante da equipe pre-formada).
/// </summary>
public class GrupoMembro
{
    public Guid Id { get; private set; }
    public Guid GrupoId { get; private set; }
    public Guid MembroId { get; private set; }

    // Navigation
    public Membro? Membro { get; private set; }

    private GrupoMembro() { }

    public static GrupoMembro Criar(Guid grupoId, Guid membroId)
    {
        return new GrupoMembro
        {
            Id = Guid.NewGuid(),
            GrupoId = grupoId,
            MembroId = membroId
        };
    }
}
