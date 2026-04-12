using Torneio.Application.DTOs.AnoTorneio;

namespace Torneio.Application.Services.Interfaces;

public interface IAnoTorneioServico
{
    Task<AnoTorneioDto?> ObterPorId(Guid id);
    Task<IEnumerable<AnoTorneioDto>> ListarPorTorneio(Guid torneioId);
    Task<AnoTorneioDto> Criar(CriarAnoTorneioDto dto);
    Task Liberar(Guid id);
    Task Finalizar(Guid id);
    Task Reabrir(Guid id);
    Task<AnoTorneioDto> ReplicarAno(Guid anoTorneioOrigemId, int novoAno);
}
