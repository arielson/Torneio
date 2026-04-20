namespace Torneio.Domain.Entities;

/// <summary>
/// Resultado do sorteio no modo GrupoEquipe:
/// um Grupo (equipe pre-formada) foi sorteado para uma Equipe (embarcação).
/// </summary>
public class SorteioGrupo
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid GrupoId { get; private set; }
    public Guid EquipeId { get; private set; }
    public int Posicao { get; private set; }

    private SorteioGrupo() { }

    public static SorteioGrupo Criar(Guid torneioId, Guid grupoId, Guid equipeId, int posicao)
    {
        return new SorteioGrupo
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            GrupoId = grupoId,
            EquipeId = equipeId,
            Posicao = posicao
        };
    }

    public void AjustarPosicao(int novaPosicao) => Posicao = novaPosicao;
}
