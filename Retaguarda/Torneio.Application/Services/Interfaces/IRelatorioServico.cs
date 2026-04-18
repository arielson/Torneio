namespace Torneio.Application.Services.Interfaces;

public interface IRelatorioServico
{
    /// <summary>Relatório de equipe — sintético (analitico=false) ou analítico (analitico=true)</summary>
    Task<byte[]> GerarRelatorioEquipe(Guid equipeId, bool analitico);

    /// <summary>Relatório de membro — sintético (analitico=false) ou analítico (analitico=true)</summary>
    Task<byte[]> GerarRelatorioMembro(Guid membroId, bool analitico);
}
