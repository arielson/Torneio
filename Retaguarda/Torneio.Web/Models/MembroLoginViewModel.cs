using System.ComponentModel.DataAnnotations;

namespace Torneio.Web.Models;

public class MembroLoginViewModel
{
    [Required(ErrorMessage = "Usuário obrigatório")]
    public string Usuario { get; set; } = null!;

    [Required(ErrorMessage = "Senha obrigatória")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = null!;

    public string? Slug { get; set; }
    public string? NomeTorneio { get; set; }
    public string? ReturnUrl { get; set; }
}
