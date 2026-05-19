using Mapster;
using OpenPeer.Application.DTOs.Comments;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPaperRepository _paperRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(ICommentRepository commentRepository, IPaperRepository paperRepository, IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _paperRepository = paperRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<CommentDto>> GetListAsync(Guid paperId, int page, int pageSize)
    {
        var (items, total) = await _commentRepository.GetPagedByPaperIdAsync(paperId, page, pageSize);

        var dtos = items.Adapt<List<CommentDto>>();

        return new PagedResponse<CommentDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<CommentDto> CreateAsync(Guid paperId, Guid userId, string content, Guid? parentId)
    {
        var paper = await _paperRepository.GetByIdAsync(paperId);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        if (parentId.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(parentId.Value);
            if (parent is null || parent.IsDeleted || parent.PaperId != paperId)
                throw new KeyNotFoundException("父评论不存在");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PaperId = paperId,
            UserId = userId,
            Content = content,
            ParentId = parentId,
            CreatedAt = DateTime.UtcNow
        };

        _commentRepository.Add(comment);
        await _unitOfWork.SaveChangesAsync();

        var dto = comment.Adapt<CommentDto>();
        dto.User = new CommentUserDto { Id = userId };
        return dto;
    }

    public async Task<CommentDto> UpdateAsync(Guid id, Guid userId, string content)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment is null || comment.IsDeleted)
            throw new KeyNotFoundException("评论不存在");

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("只能编辑自己的评论");

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;
        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync();

        return comment.Adapt<CommentDto>();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment is null || comment.IsDeleted)
            throw new KeyNotFoundException("评论不存在");

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("只能删除自己的评论");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync();
    }
}
