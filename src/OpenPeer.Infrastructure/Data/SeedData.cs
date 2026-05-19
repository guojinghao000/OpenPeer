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

        // Seed test reader user
        if (await userManager.FindByEmailAsync("alice@example.com") is null)
        {
            var reader = new User
            {
                UserName = "alice",
                Email = "alice@example.com",
                Role = UserRole.Reader,
                Bio = "计算机科学研究者，关注 AI 和数据科学领域",
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(reader, "Test1234");
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

        // Seed sample papers if empty
        if (!await context.Papers.AnyAsync())
        {
            var alice = await userManager.FindByEmailAsync("alice@example.com");
            var categories = await context.Categories.ToListAsync();

            var ai = categories.First(c => c.Name == "人工智能");
            var ds = categories.First(c => c.Name == "数据科学");

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Papers");
            Directory.CreateDirectory(uploadDir);

            // Find sample-paper.pdf relative to the solution root
            var cwd = Directory.GetCurrentDirectory();
            var sampleSource = Path.Combine(cwd, "sample-paper.pdf");
            if (!File.Exists(sampleSource))
                sampleSource = Path.Combine(cwd, "..", "sample-paper.pdf");
            if (!File.Exists(sampleSource))
                sampleSource = Path.Combine(cwd, "..", "..", "sample-paper.pdf");

            var paperId = Guid.NewGuid();
            var fileName = $"{paperId}.pdf";
            var filePath = Path.Combine(uploadDir, fileName);

            if (File.Exists(sampleSource))
            {
                File.Copy(sampleSource, filePath, overwrite: true);
            }

            var paper = new Paper
            {
                Id = paperId,
                Title = "ImageNet Classification with Deep Convolutional Neural Networks",
                Abstract = "We trained a large, deep convolutional neural network to classify the 1.2 million high-resolution images in the ImageNet LSVRC-2010 contest into the 1000 different classes. On the test data, we achieved top-1 and top-5 error rates of 37.5% and 17.0% which is considerably better than the previous state-of-the-art. The neural network, which has 60 million parameters and 650,000 neurons, consists of five convolutional layers, some of which are followed by max-pooling layers, and three fully-connected layers with a final 1000-way softmax. To make training faster, we used non-saturating neurons and a very efficient GPU implementation of the convolution operation. To reduce overfitting in the fully-connected layers we employed a recently-developed regularization method called \"dropout\" that proved to be very effective. We also entered a variant of this model in the ILSVRC-2012 competition and achieved a winning top-5 test error rate of 15.3%, compared to 26.2% achieved by the second-best entry.",
                FilePath = filePath,
                FileSize = 3682010L,
                AuthorId = alice!.Id,
                Status = PaperStatus.Published,
                PublishedAt = DateTime.UtcNow.AddDays(-30)
            };

            paper.PaperCategories.Add(new PaperCategory
            {
                PaperId = paperId,
                CategoryId = ai.Id
            });
            paper.PaperCategories.Add(new PaperCategory
            {
                PaperId = paperId,
                CategoryId = ds.Id
            });

            context.Papers.Add(paper);
            await context.SaveChangesAsync();
        }
    }
}
