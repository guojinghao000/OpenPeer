namespace OpenPeer.Application.DTOs.Ratings;

public class RatingDto
{
    public Guid Id { get; set; }
    public RatingUserDto User { get; set; } = null!;
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RatingUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
}
