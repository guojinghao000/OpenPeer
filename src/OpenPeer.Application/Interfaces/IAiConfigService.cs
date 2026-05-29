using OpenPeer.Application.DTOs.AiConfig;

namespace OpenPeer.Application.Interfaces;

public interface IAiConfigService
{
    Task<AiConfigDto> GetConfigAsync(Guid userId);
    Task UpdateConfigAsync(Guid userId, UpdateAiConfigRequest request);
}
