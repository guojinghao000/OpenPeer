using OpenPeer.Application.DTOs.SupportingData;

namespace OpenPeer.Application.Interfaces;

public interface ISupportingDataService
{
    Task<SupportingDataDto> UploadAsync(Guid paperId, Guid userId, Stream fileStream, string fileName, string contentType, long fileSize, string? description);
    Task<List<SupportingDataDto>> GetListAsync(Guid paperId);
    Task DeleteAsync(Guid paperId, Guid dataId, Guid userId);
}
