using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Comments;

namespace OpenPeer.Application.Interfaces;

public interface ICommentService
{
    Task<PagedResponse<CommentDto>> GetListAsync(Guid paperId, int page, int pageSize);
    Task<CommentDto> CreateAsync(Guid paperId, Guid userId, string content, Guid? parentId);
    Task<CommentDto> UpdateAsync(Guid id, Guid userId, string content);
    Task DeleteAsync(Guid id, Guid userId);
}
