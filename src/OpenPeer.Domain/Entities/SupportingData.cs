namespace OpenPeer.Domain.Entities;

public class SupportingData
{
    public Guid Id { get; set; }
    public Guid PaperId { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Paper Paper { get; set; } = null!;
    public User User { get; set; } = null!;
}
