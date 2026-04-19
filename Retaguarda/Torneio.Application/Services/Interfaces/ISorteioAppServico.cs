using Torneio.Application.DTOs.Sorteio;

namespace Torneio.Application.Services.Interfaces;

public interface ISorteioAppServico
{
    Task<SorteioPreCondicoesDto> VerificarPreCondicoes();
    Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio();
    Task ConfirmarSorteio(IEnumerable<ConfirmarSorteioItemDto> itens);
    Task<IEnumerable<SorteioEquipeDto>> ObterResultado();
    Task AjustarPosicao(Guid sorteioEquipeId, int novaPosicao);
    Task LimparSorteio();
}
