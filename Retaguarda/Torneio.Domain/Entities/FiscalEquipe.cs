namespace Torneio.Domain.Entities;

public class FiscalEquipe
{
    public Guid FiscalId { get; private set; }
    public Guid EquipeId { get; private set; }

    public Fiscal Fiscal { get; private set; } = null!;
    public Equipe Equipe { get; private set; } = null!;

    private FiscalEquipe() { }

    public FiscalEquipe(Guid fiscalId, Guid equipeId)
    {
        FiscalId = fiscalId;
        EquipeId = equipeId;
    }
}
