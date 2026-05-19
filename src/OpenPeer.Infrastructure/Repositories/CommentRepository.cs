using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Data;

namespace OpenPeer.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Comment> Items, int Total)> GetPagedByPaperIdAsync(Guid paperId, int page, int pageSize)
    {
        var query = _context.Comments
            .Where(c => c.PaperId == paperId && !c.IsDeleted && c.ParentId == null)
            .Include(c => c.User)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.User)
            .OrderBy(c => c.CreatedAt)
            .AsNoTracking();

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public void Add(Comment comment)
    {
        _context.Comments.Add(comment);
    }

    public void Update(Comment comment)
    {
        _context.Comments.Update(comment);
    }
}
