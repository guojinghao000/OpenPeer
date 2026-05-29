using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Infrastructure.Data.Configurations;

public class SupportingDataConfiguration : IEntityTypeConfiguration<SupportingData>
{
    public void Configure(EntityTypeBuilder<SupportingData> builder)
    {
        builder.HasKey(sd => sd.Id);
        builder.Property(sd => sd.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(sd => sd.FileName).HasMaxLength(256).IsRequired();
        builder.Property(sd => sd.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(sd => sd.FileType).HasMaxLength(50).IsRequired();
        builder.Property(sd => sd.Description).HasMaxLength(500);
        builder.Property(sd => sd.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(sd => sd.PaperId);

        builder.HasOne(sd => sd.User)
            .WithMany(u => u.SupportingData)
            .HasForeignKey(sd => sd.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
