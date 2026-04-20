namespace Torneio.Application.DTOs.Grupo;

public class GrupoDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public List<GrupoMembroDto> Membros { get; init; } = new();
}

public class GrupoMembroDto
{
    public Guid Id { get; init; }       // GrupoMembro.Id
    public Guid MembroId { get; init; }
    public string NomeMembro { get; init; } = null!;
}
