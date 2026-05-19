using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Papers;

namespace OpenPeer.Application.DTOs.Ratings;

public class RatingListResponse
{
    public List<RatingDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    public RatingDistributionDto? Distribution { get; set; }
}
