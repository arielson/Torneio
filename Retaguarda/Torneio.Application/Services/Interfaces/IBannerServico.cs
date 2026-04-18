using Torneio.Application.DTOs.Banner;
namespace Torneio.Application.Services.Interfaces;
public interface IBannerServico
{
    Task<IEnumerable<BannerDto>> ListarTodos();
    Task<IEnumerable<BannerDto>> ListarAtivos();
    Task<BannerDto?> ObterPorId(Guid id);
    Task<BannerDto> Criar(CriarBannerDto dto);
    Task Ativar(Guid id);
    Task Desativar(Guid id);
    Task Excluir(Guid id);
}
