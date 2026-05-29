using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Interfaces;

public interface IPaperRepository
{
    Task<(List<Paper> Items, int Total)> GetPagedListAsync(PaperListRequest request);
    Task<(List<Paper> Items, int Total)> GetPagedByAuthorIdAsync(Guid authorId, PaperListRequest request);
    Task<Paper?> GetByIdAsync(Guid id);
    Task<Paper?> GetDetailByIdAsync(Guid id);
    void Add(Paper paper);
    void Update(Paper paper);
    Task<List<Rating>> GetRatingDistributionAsync(Guid paperId);
}
