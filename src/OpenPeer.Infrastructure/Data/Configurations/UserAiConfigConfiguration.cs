using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Infrastructure.Data.Configurations;

public class UserAiConfigConfiguration : IEntityTypeConfiguration<UserAiConfig>
{
    public void Configure(EntityTypeBuilder<UserAiConfig> builder)
    {
        builder.HasKey(uac => uac.UserId);
        builder.Property(uac => uac.Provider).HasMaxLength(50).IsRequired();
        builder.Property(uac => uac.ApiKey).IsRequired();
        builder.Property(uac => uac.Model).HasMaxLength(100).IsRequired();
        builder.Property(uac => uac.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(uac => uac.User)
            .WithOne(u => u.AiConfig)
            .HasForeignKey<UserAiConfig>(uac => uac.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
