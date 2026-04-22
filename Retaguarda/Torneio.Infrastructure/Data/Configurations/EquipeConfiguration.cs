using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class EquipeConfiguration : IEntityTypeConfiguration<Equipe>
{
    public void Configure(EntityTypeBuilder<Equipe> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Capitao).IsRequired().HasMaxLength(200);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);
        builder.Property(e => e.FotoCapitaoUrl).HasMaxLength(500);
        builder.Property(e => e.QtdVagas).IsRequired();
        builder.Property(e => e.Custo).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Membros)
            .WithMany()
            .UsingEntity("equipe_membro");

        builder.Navigation(e => e.Membros)
            .HasField("_membros")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Fiscais)
            .HasField("_fiscais")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
