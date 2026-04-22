namespace Torneio.Application.DTOs.RegistroPublicoMembro;

public class RegistroPublicoMembroSolicitadoDto
{
    public Guid RegistroId { get; init; }
    public string CelularMascarado { get; init; } = null!;
    public DateTime ExpiraEm { get; init; }
    public string Mensagem { get; init; } = null!;
}
