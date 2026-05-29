namespace OpenPeer.Application.DTOs.AiConfig;

public class UpdateAiConfigRequest
{
    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
