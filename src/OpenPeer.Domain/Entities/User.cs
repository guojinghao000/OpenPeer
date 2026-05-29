using Microsoft.AspNetCore.Identity;
using OpenPeer.Domain.Enums;

namespace OpenPeer.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? Bio { get; set; }
    public string? AvatarPath { get; set; }
    public double ReputationScore { get; set; } = 1.0;
    public UserRole Role { get; set; } = UserRole.Reader;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Paper> Papers { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<SupportingData> SupportingData { get; set; } = [];
    public UserAiConfig? AiConfig { get; set; }
}
