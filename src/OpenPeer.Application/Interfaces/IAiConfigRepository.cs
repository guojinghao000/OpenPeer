using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Interfaces;

public interface IAiConfigRepository
{
    Task<UserAiConfig?> GetByUserIdAsync(Guid userId);
    void Add(UserAiConfig config);
    void Update(UserAiConfig config);
}
