namespace OpenPeer.Application.DTOs.Comments;

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}
