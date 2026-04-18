using Torneio.Application.DTOs.Captura;

namespace Torneio.Application.Services.Interfaces;

public interface ICapturaServico
{
    Task<CapturaDto?> ObterPorId(Guid id);
    Task<IEnumerable<CapturaDto>> ListarPorEquipe(Guid equipeId);
    Task<IEnumerable<CapturaDto>> ListarPorMembro(Guid membroId);
    Task<IEnumerable<CapturaDto>> ListarTodos();
    Task<CapturaDto> Registrar(RegistrarCapturaDto dto);
    Task Remover(Guid id);
    Task<int> SincronizarLote(IEnumerable<RegistrarCapturaDto> capturas);
}
