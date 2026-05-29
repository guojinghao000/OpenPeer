using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Data;

namespace OpenPeer.Infrastructure.Repositories;

public class PaperRepository : IPaperRepository
{
    private readonly AppDbContext _context;

    public PaperRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Paper> Items, int Total)> GetPagedListAsync(PaperListRequest request)
    {
        var query = _context.Papers
            .Where(p => p.Status == OpenPeer.Domain.Enums.PaperStatus.Published)
            .Include(p => p.Author)
            .Include(p => p.PaperCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.Comments)
            .AsNoTracking();

        if (request.AuthorId.HasValue)
        {
            query = query.Where(p => p.AuthorId == request.AuthorId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.PaperCategories.Any(pc => pc.CategoryId == request.CategoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(p =>
                EF.Functions.ToTsVector("english", p.Title + " " + p.Abstract)
                    .Matches(EF.Functions.PlainToTsQuery("english", request.Keyword)));
        }

        var total = await query.CountAsync();

        query = request.SortBy switch
        {
            "averageRating" => request.Order == "asc"
                ? query.OrderBy(p => p.AverageRating)
                : query.OrderByDescending(p => p.AverageRating),
            "commentCount" => request.Order == "asc"
                ? query.OrderBy(p => p.Comments.Count(c => !c.IsDeleted))
                : query.OrderByDescending(p => p.Comments.Count(c => !c.IsDeleted)),
            _ => request.Order == "asc"
                ? query.OrderBy(p => p.PublishedAt)
                : query.OrderByDescending(p => p.PublishedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Paper> Items, int Total)> GetPagedByAuthorIdAsync(Guid authorId, PaperListRequest request)
    {
        var query = _context.Papers
            .Where(p => p.AuthorId == authorId && !p.IsDeleted)
            .Include(p => p.Author)
            .Include(p => p.PaperCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.Comments)
            .AsNoTracking();

        var total = await query.CountAsync();

        query = request.SortBy switch
        {
            "averageRating" => request.Order == "asc"
                ? query.OrderBy(p => p.AverageRating)
                : query.OrderByDescending(p => p.AverageRating),
            "publishedAt" => request.Order == "asc"
                ? query.OrderBy(p => p.PublishedAt)
                : query.OrderByDescending(p => p.PublishedAt),
            _ => query.OrderByDescending(p => p.PublishedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Paper?> GetByIdAsync(Guid id)
    {
        return await _context.Papers
            .Include(p => p.PaperCategories)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Paper?> GetDetailByIdAsync(Guid id)
    {
        return await _context.Papers
            .Include(p => p.Author)
            .Include(p => p.PaperCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
            .Include(p => p.Ratings)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public void Add(Paper paper)
    {
        _context.Papers.Add(paper);
    }

    public void Update(Paper paper)
    {
        _context.Papers.Update(paper);
    }

    public async Task<List<Rating>> GetRatingDistributionAsync(Guid paperId)
    {
        return await _context.Ratings
            .Where(r => r.PaperId == paperId)
            .ToListAsync();
    }
}
