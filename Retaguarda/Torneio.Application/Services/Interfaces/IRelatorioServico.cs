namespace Torneio.Application.Services.Interfaces;

public interface IRelatorioServico
{
    /// <summary>Relatório de equipe — sintético (analitico=false) ou analítico (analitico=true)</summary>
    Task<byte[]> GerarRelatorioEquipe(Guid anoTorneioId, Guid equipeId, bool analitico);

    /// <summary>Relatório de membro — sintético (analitico=false) ou analítico (analitico=true)</summary>
    Task<byte[]> GerarRelatorioMembro(Guid anoTorneioId, Guid membroId, bool analitico);
}
