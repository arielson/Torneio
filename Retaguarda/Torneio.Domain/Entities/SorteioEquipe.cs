namespace Torneio.Domain.Entities;

public class SorteioEquipe
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid EquipeId { get; private set; }
    public Guid MembroId { get; private set; }
    public int Posicao { get; private set; }

    private SorteioEquipe() { }

    public static SorteioEquipe Criar(
        Guid torneioId,
        Guid equipeId,
        Guid membroId,
        int posicao)
    {
        return new SorteioEquipe
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            EquipeId = equipeId,
            MembroId = membroId,
            Posicao = posicao
        };
    }

    public void AjustarPosicao(int novaPosicao) => Posicao = novaPosicao;
}
