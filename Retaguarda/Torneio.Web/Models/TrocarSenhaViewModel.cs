using System.ComponentModel.DataAnnotations;

namespace Torneio.Web.Models;

public class TrocarSenhaViewModel
{
    [Required(ErrorMessage = "Informe a senha atual")]
    [DataType(DataType.Password)]
    public string SenhaAtual { get; set; } = null!;

    [Required(ErrorMessage = "Informe a nova senha")]
    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    [DataType(DataType.Password)]
    public string NovaSenha { get; set; } = null!;

    [Required(ErrorMessage = "Confirme a nova senha")]
    [DataType(DataType.Password)]
    public string ConfirmarSenha { get; set; } = null!;
}
