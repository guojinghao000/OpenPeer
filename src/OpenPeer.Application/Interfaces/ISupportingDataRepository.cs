using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Interfaces;

public interface ISupportingDataRepository
{
    Task<List<SupportingData>> GetByPaperIdAsync(Guid paperId);
    Task<SupportingData?> GetByIdAsync(Guid id);
    void Add(SupportingData data);
    void Remove(SupportingData data);
}
