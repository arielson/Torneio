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
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.LabelEquipe).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelEquipePlural).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelMembro).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelMembroPlural).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelSupervisor).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelSupervisorPlural).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelItem).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelItemPlural).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelCaptura).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LabelCapturaPlural).IsRequired().HasMaxLength(50);
        builder.Property(e => e.MedidaCaptura).IsRequired().HasMaxLength(20);
        builder.Property(e => e.ExibirModuloFinanceiro).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.PermitirRegistroPublicoMembro).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.ValorPorMembro).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(e => e.QuantidadeParcelas).HasDefaultValue(0);
        builder.Property(e => e.TaxaInscricaoValor).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(e => e.ModoSorteio).IsRequired();
        builder.Property(e => e.QtdGanhadores).IsRequired().HasDefaultValue(3);
        builder.Property(e => e.PremiacaoPorEquipe).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.PremiacaoPorMembro).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.TipoTorneio).IsRequired().HasDefaultValue(Torneio.Domain.Enums.TipoTorneio.Pesca);
        builder.Property(e => e.CriadoEm).IsRequired();
        builder.Property(e => e.CorPrimaria).HasMaxLength(7);

        builder.HasIndex(e => e.Slug).IsUnique();
    }
}
