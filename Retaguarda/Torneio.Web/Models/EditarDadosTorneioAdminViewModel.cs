using System.ComponentModel.DataAnnotations;

namespace Torneio.Web.Models;

public class EditarDadosTorneioAdminViewModel
{
    [Required(ErrorMessage = "O nome do torneio e obrigatorio.")]
    public string NomeTorneio { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public IFormFile? LogoArquivo { get; set; }

    [MaxLength(2000, ErrorMessage = "A descricao deve ter no maximo 2000 caracteres.")]
    public string? Descricao { get; set; }

    [MaxLength(4000, ErrorMessage = "As observacoes internas devem ter no maximo 4000 caracteres.")]
    public string? ObservacoesInternas { get; set; }

    [Range(1, 100, ErrorMessage = "Informe entre 1 e 100 ganhadores.")]
    public int QtdGanhadores { get; set; } = 3;

    public bool UsarFatorMultiplicador { get; set; }
    public bool PermitirCapturaOffline { get; set; }
    public bool ExibirModuloFinanceiro { get; set; } = true;
    public bool ExibirParticipantesPublicos { get; set; }
    public bool ExibirNaListaInicialPublica { get; set; } = true;
    public bool ExibirNaPesquisaPublica { get; set; } = true;
    public bool PremiacaoPorEquipe { get; set; } = true;
    public bool PremiacaoPorMembro { get; set; }
    public bool ApenasMaiorCapturaPorPescador { get; set; }

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor invalida. Use o formato #RRGGBB.")]
    public string? CorPrimaria { get; set; }
}
