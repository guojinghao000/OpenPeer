namespace OpenPeer.Application.DTOs.Users;

public class UserRatingItemDto
{
    public Guid Id { get; set; }
    public Guid PaperId { get; set; }
    public string PaperTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}
