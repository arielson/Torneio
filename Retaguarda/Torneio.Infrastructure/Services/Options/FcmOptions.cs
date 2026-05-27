namespace Torneio.Infrastructure.Services.Options;

public class FcmOptions
{
    public const string Section = "Fcm";

    /// Caminho para o arquivo JSON da service account do Firebase.
    public string? ServiceAccountPath { get; set; }
}
