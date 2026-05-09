using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Infrastructure.Data.Configurations;

public class PaperConfiguration : IEntityTypeConfiguration<Paper>
{
    public void Configure(EntityTypeBuilder<Paper> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Abstract).IsRequired();
        builder.Property(p => p.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Status).HasMaxLength(20).HasConversion<string>();
        builder.Property(p => p.AverageRating).HasDefaultValue(0.0);
        builder.Property(p => p.RatingCount).HasDefaultValue(0);
        builder.Property(p => p.ViewCount).HasDefaultValue(0);
        builder.Property(p => p.PublishedAt).HasDefaultValueSql("NOW()");

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasIndex(p => p.AuthorId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.PublishedAt).IsDescending();
        builder.HasIndex(p => p.AverageRating).IsDescending();

        builder.HasMany(p => p.Ratings)
            .WithOne(r => r.Paper)
            .HasForeignKey(r => r.PaperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Paper)
            .HasForeignKey(c => c.PaperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PaperCategories)
            .WithOne(pc => pc.Paper)
            .HasForeignKey(pc => pc.PaperId);
    }
}
