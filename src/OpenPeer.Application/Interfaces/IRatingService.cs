using OpenPeer.Application.DTOs.Ratings;

namespace OpenPeer.Application.Interfaces;

public interface IRatingService
{
    Task<RatingDto> SubmitOrUpdateAsync(Guid paperId, Guid userId, int score);
    Task<RatingListResponse> GetListAsync(Guid paperId, int page, int pageSize);
    Task DeleteAsync(Guid paperId, Guid userId);
}
