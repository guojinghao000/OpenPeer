namespace OpenPeer.Application.DTOs.Users;

public class AdminUserItemDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public double ReputationScore { get; set; }
    public int PaperCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
