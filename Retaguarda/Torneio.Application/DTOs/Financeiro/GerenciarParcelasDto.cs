using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Financeiro;

public class GerenciarParcelasDto
{
    public List<Guid> MembroIds { get; init; } = [];
    public bool SomenteNovos { get; init; }

    [Range(0, 999999999, ErrorMessage = "O valor por pescador nao pode ser negativo.")]
    public decimal? ValorPorMembro { get; init; }
}
