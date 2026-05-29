using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Interfaces;

public interface ICommentRepository
{
    Task<(List<Comment> Items, int Total)> GetPagedByPaperIdAsync(Guid paperId, int page, int pageSize);
    Task<(List<Comment> Items, int Total)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize);
    Task<Comment?> GetByIdAsync(Guid id);
    void Add(Comment comment);
    void Update(Comment comment);
}
