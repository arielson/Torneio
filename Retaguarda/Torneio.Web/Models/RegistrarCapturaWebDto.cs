using System.ComponentModel.DataAnnotations;

namespace Torneio.Web.Models;

public class RegistrarCapturaWebDto
{
    [Required(ErrorMessage = "Selecione uma equipe.")]
    public Guid EquipeId { get; set; }

    [Required(ErrorMessage = "Selecione um membro.")]
    public Guid MembroId { get; set; }

    [Required(ErrorMessage = "Selecione um item.")]
    public Guid ItemId { get; set; }

    [Required(ErrorMessage = "Informe o tamanho.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tamanho deve ser maior que zero.")]
    public decimal TamanhoMedida { get; set; }

    [Required(ErrorMessage = "Informe a data e hora.")]
    public DateTime DataHora { get; set; } = DateTime.Now;
}
