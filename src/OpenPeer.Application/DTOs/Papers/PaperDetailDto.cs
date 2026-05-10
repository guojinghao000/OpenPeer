namespace OpenPeer.Application.DTOs.Papers;

public class PaperDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public PaperAuthorDto Author { get; set; } = null!;
    public List<CategoryDto> Categories { get; set; } = [];
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public RatingDistributionDto? RatingDistribution { get; set; }
    public int? CurrentUserRating { get; set; }
    public int CommentCount { get; set; }
    public int ViewCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RatingDistributionDto
{
    public int Star1 { get; set; }
    public int Star2 { get; set; }
    public int Star3 { get; set; }
    public int Star4 { get; set; }
    public int Star5 { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
