namespace Torneio.Application.DTOs.Sorteio;

/// <summary>
/// Filtro opcional para o sorteio.
/// null em qualquer propriedade significa "todos".
/// </summary>
public class RealizarSorteioDto
{
    /// <summary>IDs das equipes que participarão. null = todas as equipes cadastradas.</summary>
    public List<Guid>? EquipeIds { get; init; }

    /// <summary>IDs dos membros que participarão. null = todos os membros cadastrados.</summary>
    public List<Guid>? MembroIds { get; init; }
}
