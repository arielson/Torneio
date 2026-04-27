using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class EspeciePeixeConfiguration : IEntityTypeConfiguration<EspeciePeixe>
{
    public void Configure(EntityTypeBuilder<EspeciePeixe> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.NomeCientifico).HasMaxLength(300);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);
    }
}
