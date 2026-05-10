namespace OpenPeer.Application.DTOs.Categories;

public class CategoryListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PaperCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
