namespace OpenPeer.Application.DTOs.Papers;

public class PaperDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public PaperAuthorDto Author { get; set; } = null!;
    public List<CategoryDto> Categories { get; set; } = [];
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int CommentCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}

public class PaperAuthorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
}
