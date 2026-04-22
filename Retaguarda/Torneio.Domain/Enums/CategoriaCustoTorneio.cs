using System.ComponentModel.DataAnnotations;

namespace Torneio.Domain.Enums;

public enum CategoriaCustoTorneio
{
    [Display(Name = "Embarcação")]
    Embarcacao = 0,

    [Display(Name = "Camisas")]
    Camisas = 1,

    [Display(Name = "Alimentação")]
    Alimentacao = 2,

    [Display(Name = "Combustível")]
    Combustivel = 3,

    [Display(Name = "Premiações")]
    Premiacoes = 4,

    [Display(Name = "Outros")]
    Outros = 5
}
