using OpenPeer.Domain.Enums;

namespace OpenPeer.Domain.Entities;

public class Paper
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid AuthorId { get; set; }
    public PaperStatus Status { get; set; } = PaperStatus.Published;
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User Author { get; set; } = null!;
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<PaperCategory> PaperCategories { get; set; } = [];
}
