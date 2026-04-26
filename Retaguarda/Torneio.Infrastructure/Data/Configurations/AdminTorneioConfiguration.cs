using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class AdminTorneioConfiguration : IEntityTypeConfiguration<AdminTorneio>
{
    public void Configure(EntityTypeBuilder<AdminTorneio> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Usuario).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SenhaHash).IsRequired().HasMaxLength(200);
        builder.Property(e => e.DeveAlterarSenha).IsRequired().HasDefaultValue(false);

        builder.HasIndex(e => new { e.Usuario, e.TorneioId }).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
