using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class RegistroPublicoMembroConfiguration : IEntityTypeConfiguration<RegistroPublicoMembro>
{
    public void Configure(EntityTypeBuilder<RegistroPublicoMembro> builder)
    {
        builder.ToTable("registros_publicos_membros");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Celular).IsRequired().HasMaxLength(30);
        builder.Property(e => e.CelularNormalizado).IsRequired().HasMaxLength(20);
        builder.Property(e => e.TamanhoCamisa).HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.QuantidadeEnvios).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.TentativasValidacao).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.CriadoEm).IsRequired();
        builder.Property(e => e.UltimoEnvioEm).IsRequired();
        builder.Property(e => e.ExpiraEm).IsRequired();

        builder.HasIndex(e => new { e.TorneioId, e.CelularNormalizado, e.CriadoEm });
    }
}
