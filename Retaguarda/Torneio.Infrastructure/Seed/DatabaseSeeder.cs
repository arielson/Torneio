using Microsoft.EntityFrameworkCore;
using Torneio.Application.Common;
using Torneio.Domain.Entities;
using Torneio.Infrastructure.Data;

namespace Torneio.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(TorneioDbContext context, IPasswordHasher passwordHasher)
    {
        // Garante que o banco existe e as migrations foram aplicadas
        await context.Database.MigrateAsync();

        if (!await context.AdminsGeral.AnyAsync())
        {
            var admin = AdminGeral.Criar(
                nome: "Administrador",
                usuario: "admin",
                senhaHash: passwordHasher.Hash("admin123"));

            context.AdminsGeral.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
