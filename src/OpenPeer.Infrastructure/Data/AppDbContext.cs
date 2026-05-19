using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Paper> Papers => Set<Paper>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PaperCategory> PaperCategories => Set<PaperCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
