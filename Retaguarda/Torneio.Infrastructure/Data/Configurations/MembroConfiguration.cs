using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class MembroConfiguration : IEntityTypeConfiguration<Membro>
{
    public void Configure(EntityTypeBuilder<Membro> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);
        builder.Property(e => e.Celular).HasMaxLength(30);
        builder.Property(e => e.TamanhoCamisa).HasMaxLength(20);
        builder.Property(e => e.Usuario).HasMaxLength(100);
        builder.Property(e => e.SenhaHash).HasMaxLength(500);
        builder.Property(e => e.DeveAlterarSenha).IsRequired().HasDefaultValue(false);

        builder.HasIndex(e => new { e.TorneioId, e.Usuario }).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
