using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Data;

namespace OpenPeer.Infrastructure.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rating?> GetByUserAndPaperAsync(Guid userId, Guid paperId)
    {
        return await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.PaperId == paperId);
    }

    public async Task<(List<Rating> Items, int Total)> GetPagedByPaperIdAsync(Guid paperId, int page, int pageSize)
    {
        var query = _context.Ratings
            .Where(r => r.PaperId == paperId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking();

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Rating>> GetAllByPaperIdAsync(Guid paperId)
    {
        return await _context.Ratings
            .Where(r => r.PaperId == paperId)
            .ToListAsync();
    }

    public void Add(Rating rating)
    {
        _context.Ratings.Add(rating);
    }

    public void Update(Rating rating)
    {
        _context.Ratings.Update(rating);
    }

    public void Remove(Rating rating)
    {
        _context.Ratings.Remove(rating);
    }
}
