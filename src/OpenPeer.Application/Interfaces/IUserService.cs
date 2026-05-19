using OpenPeer.Application.DTOs.Users;

namespace OpenPeer.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserProfileDto?> GetPublicProfileAsync(Guid userId);
}
