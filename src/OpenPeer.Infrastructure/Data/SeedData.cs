using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenPeer.Domain.Entities;
using OpenPeer.Domain.Enums;

namespace OpenPeer.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var context = services.GetRequiredService<AppDbContext>();

        // Seed admin user
        if (await userManager.FindByEmailAsync("admin@openpeer.com") is null)
        {
            var admin = new User
            {
                UserName = "admin",
                Email = "admin@openpeer.com",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin1234");
        }

        // Seed categories if empty
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "人工智能", Description = "机器学习、深度学习、自然语言处理等" },
                new Category { Name = "数据科学", Description = "数据分析、统计学、大数据等" },
                new Category { Name = "计算机系统", Description = "操作系统、计算机网络、分布式系统等" },
                new Category { Name = "软件工程", Description = "软件架构、开发方法、DevOps 等" },
                new Category { Name = "数学", Description = "计算数学、优化、数值分析等" },
                new Category { Name = "物理学", Description = "计算物理、量子计算、材料科学等" },
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
