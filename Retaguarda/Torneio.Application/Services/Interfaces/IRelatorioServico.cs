namespace Torneio.Application.Services.Interfaces;

public interface IRelatorioServico
{
    Task<byte[]> GerarRelatorioEquipe(Guid equipeId, bool analitico);

    Task<byte[]> GerarRelatorioMembro(Guid membroId, bool analitico);

    Task<byte[]> GerarRelatorioGanhadores(
        int quantidadeEquipes,
        int quantidadeMembrosPontuacao,
        int quantidadeMembrosMaiorCaptura,
        bool analitico);

    Task<byte[]> GerarRelatorioMaioresCapturas(int quantidade);
}
