using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Interfaces;

public interface IRatingRepository
{
    Task<Rating?> GetByUserAndPaperAsync(Guid userId, Guid paperId);
    Task<(List<Rating> Items, int Total)> GetPagedByPaperIdAsync(Guid paperId, int page, int pageSize);
    Task<(List<Rating> Items, int Total)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize);
    Task<List<Rating>> GetAllByPaperIdAsync(Guid paperId);
    void Add(Rating rating);
    void Update(Rating rating);
    void Remove(Rating rating);
}
