using Microsoft.Extensions.Logging;
using OpenPeer.Application.DTOs.AiConfig;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Services;

public class AiConfigService : IAiConfigService
{
    private readonly IAiConfigRepository _repository;
    private readonly IEncryptionService _encryption;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AiConfigService> _logger;

    public AiConfigService(
        IAiConfigRepository repository,
        IEncryptionService encryption,
        IUnitOfWork unitOfWork,
        ILogger<AiConfigService> logger)
    {
        _repository = repository;
        _encryption = encryption;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AiConfigDto> GetConfigAsync(Guid userId)
    {
        var config = await _repository.GetByUserIdAsync(userId);

        if (config is null)
            return new AiConfigDto();

        return new AiConfigDto
        {
            Provider = config.Provider,
            Model = config.Model,
            HasApiKey = !string.IsNullOrEmpty(config.ApiKey)
        };
    }

    public async Task UpdateConfigAsync(Guid userId, UpdateAiConfigRequest request)
    {
        var config = await _repository.GetByUserIdAsync(userId);

        var encryptedKey = _encryption.Encrypt(request.ApiKey);

        if (config is null)
        {
            config = new UserAiConfig
            {
                UserId = userId,
                Provider = request.Provider,
                ApiKey = encryptedKey,
                Model = request.Model,
                CreatedAt = DateTime.UtcNow
            };
            _repository.Add(config);
        }
        else
        {
            config.Provider = request.Provider;
            config.ApiKey = encryptedKey;
            config.Model = request.Model;
            config.UpdatedAt = DateTime.UtcNow;
            _repository.Update(config);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("AI config updated for user {UserId}, provider: {Provider}", userId, request.Provider);
    }
}
