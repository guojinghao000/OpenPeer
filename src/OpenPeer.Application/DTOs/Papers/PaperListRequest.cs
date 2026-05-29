namespace OpenPeer.Application.DTOs.Papers;

public class PaperListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "publishedAt";
    public string Order { get; set; } = "desc";
    public Guid? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public Guid? AuthorId { get; set; }
}
