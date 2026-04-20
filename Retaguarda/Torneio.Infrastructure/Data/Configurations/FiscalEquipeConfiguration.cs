using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class FiscalEquipeConfiguration : IEntityTypeConfiguration<FiscalEquipe>
{
    public void Configure(EntityTypeBuilder<FiscalEquipe> builder)
    {
        builder.ToTable("fiscal_equipe");
        builder.HasKey(x => new { x.FiscalId, x.EquipeId });

        builder.HasOne(x => x.Fiscal)
            .WithMany(x => x.Equipes)
            .HasForeignKey(x => x.FiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Equipe)
            .WithMany(x => x.Fiscais)
            .HasForeignKey(x => x.EquipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
