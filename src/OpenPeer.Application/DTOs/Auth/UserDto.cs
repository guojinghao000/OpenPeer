namespace OpenPeer.Application.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarPath { get; set; }
    public double ReputationScore { get; set; }
    public string Role { get; set; } = string.Empty;
    public int PaperCount { get; set; }
    public int RatingCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
