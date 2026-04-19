using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;

namespace Torneio.Infrastructure.Data.Configurations;

public class CapturaConfiguration : IEntityTypeConfiguration<Captura>
{
    public void Configure(EntityTypeBuilder<Captura> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TamanhoMedida).HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(e => e.FotoUrl).HasMaxLength(500);
        builder.Property(e => e.DataHora).IsRequired();
        builder.Property(e => e.Origem)
            .IsRequired()
            .HasDefaultValue(OrigemCaptura.App);
        builder.Property(e => e.FonteFoto);
        builder.Property(e => e.Invalidada)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(e => e.MotivoInvalidacao)
            .HasMaxLength(500);

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
