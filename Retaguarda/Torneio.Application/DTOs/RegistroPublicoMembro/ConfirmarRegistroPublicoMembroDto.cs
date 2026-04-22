using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.RegistroPublicoMembro;

public class ConfirmarRegistroPublicoMembroDto
{
    [Required(ErrorMessage = "O identificador do registro e obrigatorio.")]
    public Guid RegistroId { get; init; }

    [Required(ErrorMessage = "O codigo e obrigatorio.")]
    public string Codigo { get; init; } = null!;

    public string? Usuario { get; init; }
    public string? Senha { get; init; }
}
