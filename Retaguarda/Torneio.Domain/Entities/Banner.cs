using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class Banner
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string ImagemUrl { get; private set; } = null!;
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; }

    /// <summary>Tipo de destino ao clicar no banner.</summary>
    public TipoDestinoBanner TipoDestino { get; private set; } = TipoDestinoBanner.Torneio;

    /// <summary>
    /// Valor do destino conforme TipoDestino:
    /// Torneio → null (usa slug do TorneioId)
    /// Site → URL completa (https://...)
    /// WhatsApp → número internacional sem +/espaços (ex: 5511999999999)
    /// Instagram → handle sem @ (ex: usuario)
    /// Email → endereço de e-mail
    /// Nenhum → null
    /// </summary>
    public string? Destino { get; private set; }

    public TorneioEntity? Torneio { get; private set; }

    private Banner() { }

    public static Banner Criar(Guid torneioId, string imagemUrl, int ordem,
        TipoDestinoBanner tipoDestino = TipoDestinoBanner.Torneio, string? destino = null) => new()
    {
        Id = Guid.NewGuid(),
        TorneioId = torneioId,
        ImagemUrl = imagemUrl,
        Ordem = ordem,
        Ativo = true,
        TipoDestino = tipoDestino,
        Destino = destino?.Trim(),
    };

    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;

    public void Atualizar(string imagemUrl, int ordem,
        TipoDestinoBanner tipoDestino = TipoDestinoBanner.Torneio, string? destino = null)
    {
        ImagemUrl = imagemUrl;
        Ordem = ordem;
        TipoDestino = tipoDestino;
        Destino = destino?.Trim();
    }
}
