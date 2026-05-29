namespace OpenPeer.Application.DTOs.Users;

public class UserCommentItemDto
{
    public Guid Id { get; set; }
    public Guid PaperId { get; set; }
    public string PaperTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
