using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Infrastructure.Data.Configurations;

public class PaperCategoryConfiguration : IEntityTypeConfiguration<PaperCategory>
{
    public void Configure(EntityTypeBuilder<PaperCategory> builder)
    {
        builder.HasKey(pc => new { pc.PaperId, pc.CategoryId });

        builder.HasIndex(pc => pc.CategoryId);
    }
}
