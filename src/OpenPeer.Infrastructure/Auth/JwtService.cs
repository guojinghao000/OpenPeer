using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenPeer.Application.DTOs.Auth;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Infrastructure.Auth;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    private readonly Dictionary<string, TokenEntry> _refreshTokens = [];

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<TokenResponse> GenerateTokensAsync(Guid userId, string userName, string role)
    {
        var accessToken = GenerateAccessToken(userId, userName, role);
        var refreshToken = GenerateRefreshToken(userId);

        var expiresIn = (int)TimeSpan.FromMinutes(_options.AccessTokenExpirationMinutes).TotalSeconds;

        var response = new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        return Task.FromResult(response);
    }

    public Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var entry) || entry.ExpiresAt < DateTime.UtcNow)
        {
            _refreshTokens.Remove(refreshToken);
            return Task.FromResult<TokenResponse?>(null);
        }

        _refreshTokens.Remove(refreshToken);

        var response = new TokenResponse
        {
            AccessToken = GenerateAccessToken(entry.UserId, entry.UserName, entry.Role),
            RefreshToken = GenerateRefreshToken(entry.UserId),
            ExpiresIn = (int)TimeSpan.FromMinutes(_options.AccessTokenExpirationMinutes).TotalSeconds
        };

        return Task.FromResult<TokenResponse?>(response);
    }

    public Task RevokeTokenAsync(string refreshToken)
    {
        _refreshTokens.Remove(refreshToken);
        return Task.CompletedTask;
    }

    public Guid? GetUserIdFromExpiredToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.Secret);

        try
        {
            tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                ValidateLifetime = false
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim is not null ? Guid.Parse(userIdClaim.Value) : null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateAccessToken(Guid userId, string userName, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);

        _refreshTokens[refreshToken] = new TokenEntry
        {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays),
            UserName = string.Empty,
            Role = string.Empty
        };

        return refreshToken;
    }

    private class TokenEntry
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
