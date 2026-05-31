using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Asaas;

public class SalvarConfiguracaoAsaasDto
{
    public Guid TorneioId { get; set; }

    [Required(ErrorMessage = "A chave de API do Asaas é obrigatória.")]
    [StringLength(200, ErrorMessage = "A chave não pode ultrapassar 200 caracteres.")]
    public string ChaveApiAsaas { get; set; } = null!;

    public bool AceitarPix { get; set; }
    public bool AceitarCartaoCredito { get; set; }
}
