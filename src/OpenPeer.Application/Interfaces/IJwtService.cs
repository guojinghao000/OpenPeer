using OpenPeer.Application.DTOs.Auth;

namespace OpenPeer.Application.Interfaces;

public interface IJwtService
{
    Task<TokenResponse> GenerateTokensAsync(Guid userId, string userName, string role);
    Task<TokenResponse?> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Guid? GetUserIdFromExpiredToken(string accessToken);
}
