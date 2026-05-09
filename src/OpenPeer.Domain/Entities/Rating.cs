namespace OpenPeer.Domain.Entities;

public class Rating
{
    public Guid Id { get; set; }
    public Guid PaperId { get; set; }
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Paper Paper { get; set; } = null!;
    public User User { get; set; } = null!;
}
