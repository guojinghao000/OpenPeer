namespace OpenPeer.Application.DTOs.Papers;

public class CreatePaperRequest
{
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public List<Guid>? CategoryIds { get; set; }
}
