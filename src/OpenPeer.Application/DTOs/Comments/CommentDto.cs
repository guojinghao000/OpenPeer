namespace OpenPeer.Application.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public CommentUserDto User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public List<CommentDto> Replies { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CommentUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
}
