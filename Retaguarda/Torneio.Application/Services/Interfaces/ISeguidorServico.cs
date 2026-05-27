namespace Torneio.Application.Services.Interfaces;

public interface ISeguidorServico
{
    Task RegistrarAsync(Guid torneioId, string deviceToken, string plataforma);
    Task RemoverAsync(Guid torneioId, string deviceToken);
}
