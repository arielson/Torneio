using Torneio.Application.DTOs.Captura;

namespace Torneio.Application.Services.Interfaces;

public interface ICapturaServico
{
    Task<CapturaDto?> ObterPorId(Guid id);
    Task<IEnumerable<CapturaDto>> ListarPorEquipe(Guid equipeId, Guid anoTorneioId);
    Task<IEnumerable<CapturaDto>> ListarPorMembro(Guid membroId, Guid anoTorneioId);
    Task<IEnumerable<CapturaDto>> ListarPorAnoTorneio(Guid anoTorneioId);
    Task<CapturaDto> Registrar(RegistrarCapturaDto dto);
    Task Remover(Guid id);
    Task<int> SincronizarLote(IEnumerable<RegistrarCapturaDto> capturas);
}
