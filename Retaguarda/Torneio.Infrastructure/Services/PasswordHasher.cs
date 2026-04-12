using Torneio.Application.Common;

namespace Torneio.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string senha) =>
        BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) =>
        BCrypt.Net.BCrypt.Verify(senha, hash);
}
