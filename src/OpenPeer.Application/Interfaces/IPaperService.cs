using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Papers;

namespace OpenPeer.Application.Interfaces;

public interface IPaperService
{
    Task<PagedResponse<PaperDto>> GetListAsync(PaperListRequest request);
    Task<PaperDetailDto> GetDetailAsync(Guid id, Guid? currentUserId);
    Task<PaperDto> CreateAsync(CreatePaperRequest request, Stream fileStream, string fileName, string contentType, long fileSize, Guid authorId);
    Task<PaperDto> UpdateAsync(Guid id, UpdatePaperRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task RetractAsync(Guid id, string reason, Guid userId);
}
