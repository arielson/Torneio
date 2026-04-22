using Torneio.Application.DTOs.Financeiro;

namespace Torneio.Web.Models;

public class FinanceiroDashboardViewModel
{
    public IndicadoresFinanceiroDto Indicadores { get; init; } = new();
}
