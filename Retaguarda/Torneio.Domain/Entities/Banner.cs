using Torneio.Domain.Entities;

namespace Torneio.Domain.Entities;

public class Banner
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string ImagemUrl { get; private set; } = null!;
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; }
    public TorneioEntity? Torneio { get; private set; }

    private Banner() { }

    public static Banner Criar(Guid torneioId, string imagemUrl, int ordem) => new()
    {
        Id = Guid.NewGuid(),
        TorneioId = torneioId,
        ImagemUrl = imagemUrl,
        Ordem = ordem,
        Ativo = true,
    };

    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;
    public void Atualizar(string imagemUrl, int ordem) { ImagemUrl = imagemUrl; Ordem = ordem; }
}
