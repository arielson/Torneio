namespace Torneio.Application.DTOs.Asaas;

public class PixQrCodeDto
{
    public string EncodedImage { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public string? ExpirationDate { get; init; }
}
