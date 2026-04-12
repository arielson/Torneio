using System.ComponentModel.DataAnnotations;

namespace Torneio.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Usuário obrigatório")]
    public string Usuario { get; set; } = null!;

    [Required(ErrorMessage = "Senha obrigatória")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = null!;

    public string? ReturnUrl { get; set; }
}
