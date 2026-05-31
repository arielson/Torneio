using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ConfiguracaoAsaasTorneioConfiguration : IEntityTypeConfiguration<ConfiguracaoAsaasTorneio>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoAsaasTorneio> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StatusChave).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.ChaveApiAsaas).HasMaxLength(200);
        builder.Property(e => e.AsaasAccountId).HasMaxLength(100);
        builder.Property(e => e.AceitarPix).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.AceitarCartaoCredito).IsRequired().HasDefaultValue(false);

        builder.HasIndex(e => e.TorneioId).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
