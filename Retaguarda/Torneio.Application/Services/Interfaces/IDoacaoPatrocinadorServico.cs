using Torneio.Application.DTOs.Financeiro;

namespace Torneio.Application.Services.Interfaces;

public interface IDoacaoPatrocinadorServico
{
    Task<IEnumerable<DoacaoPatrocinadorDto>> Listar(Guid torneioId);
    Task<DoacaoPatrocinadorDto?> ObterPorId(Guid id);
    Task<DoacaoPatrocinadorDto> Criar(CriarDoacaoPatrocinadorDto dto);
    Task Atualizar(Guid id, AtualizarDoacaoPatrocinadorDto dto);
    Task Remover(Guid id);
}
