using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class SeguidorTorneioConfiguration : IEntityTypeConfiguration<SeguidorTorneio>
{
    public void Configure(EntityTypeBuilder<SeguidorTorneio> builder)
    {
        builder.ToTable("seguidor_torneio");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TorneioId).IsRequired();
        builder.Property(e => e.DeviceToken).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Plataforma).IsRequired().HasMaxLength(20);
        builder.Property(e => e.CriadoEm).IsRequired();

        builder.HasIndex(e => new { e.TorneioId, e.DeviceToken }).IsUnique();
    }
}
