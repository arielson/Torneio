using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ChecklistTorneioItemConfiguration : IEntityTypeConfiguration<ChecklistTorneioItem>
{
    public void Configure(EntityTypeBuilder<ChecklistTorneioItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Item).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Responsavel).HasMaxLength(200);
    }
}
