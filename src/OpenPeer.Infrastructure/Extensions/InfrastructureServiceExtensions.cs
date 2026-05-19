using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenPeer.Application.Interfaces;
using OpenPeer.Application.Services;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Auth;
using OpenPeer.Infrastructure.Data;
using OpenPeer.Infrastructure.Repositories;
using OpenPeer.Infrastructure.Storage;

namespace OpenPeer.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IPaperRepository, PaperRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IPaperService, PaperService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
