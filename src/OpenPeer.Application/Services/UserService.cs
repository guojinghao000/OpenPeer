using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.DTOs.Users;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Papers)
            .Include(u => u.Ratings)
            .Include(u => u.Comments)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new KeyNotFoundException("用户不存在");

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Bio = user.Bio,
            AvatarPath = user.AvatarPath,
            ReputationScore = user.ReputationScore,
            Role = user.Role.ToString(),
            PaperCount = user.Papers.Count(p => !p.IsDeleted),
            RatingCount = user.Ratings.Count,
            CommentCount = user.Comments.Count(c => !c.IsDeleted),
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new KeyNotFoundException("用户不存在");

        if (request.Bio is not null)
        {
            if (request.Bio.Length > 500)
                throw new InvalidOperationException("个人简介不能超过500字符");
            user.Bio = request.Bio;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new KeyNotFoundException("用户不存在");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"密码修改失败: {errors}");
        }
    }

    public async Task<UserProfileDto?> GetPublicProfileAsync(Guid userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Papers)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Bio = user.Bio,
            AvatarPath = user.AvatarPath,
            ReputationScore = user.ReputationScore,
            Role = user.Role.ToString(),
            PaperCount = user.Papers.Count(p => !p.IsDeleted),
            RatingCount = 0,
            CommentCount = 0,
            CreatedAt = user.CreatedAt
        };
    }
}
