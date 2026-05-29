namespace OpenPeer.Application.DTOs.AiConfig;

public class AiConfigDto
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool HasApiKey { get; set; }
}
