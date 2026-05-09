using Mapster;
using Microsoft.AspNetCore.Identity;
using OpenPeer.Application.DTOs.Auth;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Domain.Enums;

namespace OpenPeer.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;

    public AuthService(UserManager<User> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("该邮箱已被注册");

        existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
            throw new InvalidOperationException("该用户名已被使用");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            Role = UserRole.Reader,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"注册失败: {errors}");
        }

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            ReputationScore = user.ReputationScore,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<(UserDto User, TokenResponse Token)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            throw new UnauthorizedAccessException("邮箱或密码错误");

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
            throw new UnauthorizedAccessException("邮箱或密码错误");

        var token = await _jwtService.GenerateTokensAsync(user.Id, user.UserName!, user.Role.ToString());

        var userDto = user.Adapt<UserDto>();

        return (userDto, token);
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _jwtService.RefreshTokenAsync(refreshToken);
        if (token is null)
            throw new UnauthorizedAccessException("Refresh Token 无效或已过期");

        return token;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _jwtService.RevokeTokenAsync(refreshToken);
    }
}
