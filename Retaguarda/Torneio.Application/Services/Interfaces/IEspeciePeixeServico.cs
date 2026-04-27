using Torneio.Application.DTOs.EspeciePeixe;

namespace Torneio.Application.Services.Interfaces;

public interface IEspeciePeixeServico
{
    Task<IEnumerable<EspeciePeixeDto>> ListarTodas();
    Task<EspeciePeixeDto?> ObterPorId(Guid id);
    Task<EspeciePeixeDto> Criar(CriarEspeciePeixeDto dto);
    Task Atualizar(Guid id, AtualizarEspeciePeixeDto dto);
    Task Remover(Guid id);
}
