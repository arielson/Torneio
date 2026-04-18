using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class CapturaConfiguration : IEntityTypeConfiguration<Captura>
{
    public void Configure(EntityTypeBuilder<Captura> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TamanhoMedida).HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(e => e.FotoUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.DataHora).IsRequired();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Membro>()
            .WithMany()
            .HasForeignKey(e => e.MembroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipe>()
            .WithMany()
            .HasForeignKey(e => e.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TorneioId, e.EquipeId });
        builder.HasIndex(e => new { e.TorneioId, e.MembroId });
        builder.HasIndex(e => e.PendenteSync);
    }
}
