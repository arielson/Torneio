using Torneio.Application.DTOs.Sorteio;

namespace Torneio.Application.Services.Interfaces;

public interface ISorteioGrupoAppServico
{
    Task<SorteioGrupoPreCondicoesDto> VerificarPreCondicoes();
    Task<IEnumerable<SorteioGrupoDto>> RealizarSorteio(RealizarSorteioGrupoDto? filtro = null);
    Task ConfirmarSorteio(IEnumerable<ConfirmarSorteioGrupoItemDto> itens);
    Task<IEnumerable<SorteioGrupoDto>> ObterResultado();
    Task AjustarPosicao(Guid sorteioGrupoId, int novaPosicao);
    Task LimparSorteio();
}
