using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Torneio;

public class CriarTorneioDto
{
    [Required(ErrorMessage = "O slug é obrigatório.")]
    public string Slug { get; init; } = null!;

    [Required(ErrorMessage = "O nome do torneio é obrigatório.")]
    public string NomeTorneio { get; init; } = null!;

    public string? LogoUrl { get; init; }
    public TipoTorneio TipoTorneio { get; init; } = TipoTorneio.Pesca;
    public bool UsarFatorMultiplicador { get; init; } = false;
    public bool PermitirCapturaOffline { get; init; } = true;
    public ModoSorteio ModoSorteio { get; init; }
}
