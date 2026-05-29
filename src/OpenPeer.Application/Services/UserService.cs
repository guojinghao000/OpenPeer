using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.DTOs.Users;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IPaperRepository _paperRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly ICommentRepository _commentRepository;

    public UserService(
        UserManager<User> userManager,
        IPaperRepository paperRepository,
        IRatingRepository ratingRepository,
        ICommentRepository commentRepository)
    {
        _userManager = userManager;
        _paperRepository = paperRepository;
        _ratingRepository = ratingRepository;
        _commentRepository = commentRepository;
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

    public async Task<PagedResponse<UserRatingItemDto>> GetMyRatingsAsync(Guid userId, int page, int pageSize)
    {
        var (items, total) = await _ratingRepository.GetPagedByUserIdAsync(userId, page, pageSize);

        var dtoItems = items.Select(r => new UserRatingItemDto
        {
            Id = r.Id,
            PaperId = r.PaperId,
            PaperTitle = r.Paper.Title,
            Score = r.Score,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResponse<UserRatingItemDto>
        {
            Items = dtoItems,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PagedResponse<UserCommentItemDto>> GetMyCommentsAsync(Guid userId, int page, int pageSize)
    {
        var (items, total) = await _commentRepository.GetPagedByUserIdAsync(userId, page, pageSize);

        var dtoItems = items.Select(c => new UserCommentItemDto
        {
            Id = c.Id,
            PaperId = c.PaperId,
            PaperTitle = c.Paper.Title,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        return new PagedResponse<UserCommentItemDto>
        {
            Items = dtoItems,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PagedResponse<PaperDto>> GetMyPapersAsync(Guid userId, int page, int pageSize)
    {
        var request = new PaperListRequest { Page = page, PageSize = pageSize };
        var (items, total) = await _paperRepository.GetPagedByAuthorIdAsync(userId, request);

        var dtoItems = items.Adapt<List<PaperDto>>();

        return new PagedResponse<PaperDto>
        {
            Items = dtoItems,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PagedResponse<AdminUserItemDto>> GetAdminUserListAsync(int page, int pageSize, string? search)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.UserName!.Contains(search) || u.Email!.Contains(search));
        }

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(u => u.Papers)
            .ToListAsync();

        var items = users.Select(u => new AdminUserItemDto
        {
            Id = u.Id,
            UserName = u.UserName!,
            Email = u.Email!,
            Role = u.Role.ToString(),
            Bio = u.Bio,
            ReputationScore = u.ReputationScore,
            PaperCount = u.Papers.Count(p => !p.IsDeleted),
            CreatedAt = u.CreatedAt
        }).ToList();

        return new PagedResponse<AdminUserItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task UpdateUserRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new KeyNotFoundException("用户不存在");

        if (!Enum.TryParse<Domain.Enums.UserRole>(role, out var userRole))
            throw new ArgumentException("无效的角色值");

        user.Role = userRole;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    public async Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string fileName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new KeyNotFoundException("用户不存在");

        var ext = Path.GetExtension(fileName);
        var avatarFileName = $"{userId}{ext}";
        var avatarDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Avatars");
        Directory.CreateDirectory(avatarDir);
        var filePath = Path.Combine(avatarDir, avatarFileName);

        await using var output = File.Create(filePath);
        await fileStream.CopyToAsync(output);

        var relativePath = Path.Combine("Uploads", "Avatars", avatarFileName);
        user.AvatarPath = relativePath;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return relativePath;
    }
}
