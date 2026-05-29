namespace OpenPeer.Application.DTOs.AiConfig;

public class GenerateLatexRequest
{
    public string Title { get; set; } = string.Empty;
    public List<Guid> DataIds { get; set; } = [];
    public string Prompt { get; set; } = string.Empty;
}
