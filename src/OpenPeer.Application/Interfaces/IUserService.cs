using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.DTOs.Users;

namespace OpenPeer.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserProfileDto?> GetPublicProfileAsync(Guid userId);
    Task<PagedResponse<UserRatingItemDto>> GetMyRatingsAsync(Guid userId, int page, int pageSize);
    Task<PagedResponse<UserCommentItemDto>> GetMyCommentsAsync(Guid userId, int page, int pageSize);
    Task<PagedResponse<PaperDto>> GetMyPapersAsync(Guid userId, int page, int pageSize);
    Task<PagedResponse<AdminUserItemDto>> GetAdminUserListAsync(int page, int pageSize, string? search);
    Task UpdateUserRoleAsync(Guid userId, string role);
    Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string fileName);
}
