using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class PatrocinadorConfiguration : IEntityTypeConfiguration<Patrocinador>
{
    public void Configure(EntityTypeBuilder<Patrocinador> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.FotoUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Instagram).HasMaxLength(200);
        builder.Property(e => e.Site).HasMaxLength(300);
        builder.Property(e => e.Zap).HasMaxLength(50);

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
