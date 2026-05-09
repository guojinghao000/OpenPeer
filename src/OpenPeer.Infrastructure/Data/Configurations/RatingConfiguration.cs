using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(r => new { r.PaperId, r.UserId }).IsUnique();
        builder.HasIndex(r => r.PaperId);
        builder.HasIndex(r => r.UserId);
    }
}
