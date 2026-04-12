using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class AdminGeralConfiguration : IEntityTypeConfiguration<AdminGeral>
{
    public void Configure(EntityTypeBuilder<AdminGeral> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Usuario).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SenhaHash).IsRequired().HasMaxLength(200);

        builder.HasIndex(e => e.Usuario).IsUnique();
    }
}
