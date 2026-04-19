using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;
namespace Torneio.Infrastructure.Data.Configurations;
public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ImagemUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Ordem).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.TipoDestino).IsRequired().HasDefaultValue(Torneio.Domain.Enums.TipoDestinoBanner.Torneio);
        builder.Property(e => e.Destino).HasMaxLength(500);
        builder.HasOne(e => e.Torneio)
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.Ativo, e.Ordem });
    }
}
