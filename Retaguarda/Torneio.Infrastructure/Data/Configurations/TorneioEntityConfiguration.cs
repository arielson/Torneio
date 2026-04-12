using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class TorneioEntityConfiguration : IEntityTypeConfiguration<TorneioEntity>
{
    public void Configure(EntityTypeBuilder<TorneioEntity> builder)
    {
        builder.ToTable("torneio");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Slug).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NomeTorneio).IsRequired().HasMaxLength(200);
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.LabelEquipe).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelMembro).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelSupervisor).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelItem).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelCaptura).IsRequired().HasMaxLength(50);
        builder.Property(e => e.MedidaCaptura).IsRequired().HasMaxLength(20);
        builder.Property(e => e.ModoSorteio).IsRequired();
        builder.Property(e => e.TipoTorneio).IsRequired().HasDefaultValue(Torneio.Domain.Enums.TipoTorneio.Pesca);

        builder.HasIndex(e => e.Slug).IsUnique();
    }
}
