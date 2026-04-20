using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Equipe;

public class ReorganizacaoEmergencialEquipeDto
{
    [Required(ErrorMessage = "Selecione o membro que sera reorganizado.")]
    public Guid MembroId { get; init; }

    [Required(ErrorMessage = "Selecione a embarcacao de destino.")]
    public Guid EquipeDestinoId { get; init; }

    [Required(ErrorMessage = "Informe o motivo da reorganizacao emergencial.")]
    [MaxLength(1000, ErrorMessage = "O motivo deve ter no maximo 1000 caracteres.")]
    public string Motivo { get; init; } = null!;

    [Required(ErrorMessage = "Confirme a operacao digitando REORGANIZAR.")]
    public string Confirmacao { get; init; } = null!;
}
