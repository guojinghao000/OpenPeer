using Mapster;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Domain.Enums;

namespace OpenPeer.Application.Services;

public class PaperService : IPaperService
{
    private readonly IPaperRepository _paperRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileStorageService _fileStorageService;

    public PaperService(
        IPaperRepository paperRepository,
        ICategoryRepository categoryRepository,
        IFileStorageService fileStorageService)
    {
        _paperRepository = paperRepository;
        _categoryRepository = categoryRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResponse<PaperDto>> GetListAsync(PaperListRequest request)
    {
        var (items, total) = await _paperRepository.GetPagedListAsync(request);

        var dtos = items.Select(p =>
        {
            var dto = p.Adapt<PaperDto>();
            dto.CommentCount = p.Comments.Count(c => !c.IsDeleted);
            return dto;
        }).ToList();

        return new PagedResponse<PaperDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total
        };
    }

    public async Task<PaperDetailDto> GetDetailAsync(Guid id, Guid? currentUserId)
    {
        var paper = await _paperRepository.GetDetailByIdAsync(id);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        var dto = paper.Adapt<PaperDetailDto>();
        dto.FileUrl = $"/api/files/papers/{Path.GetFileName(paper.FilePath)}";
        dto.CommentCount = paper.Comments.Count(c => !c.IsDeleted);

        paper.ViewCount++;
        _paperRepository.Update(paper);

        if (currentUserId.HasValue)
        {
            var userRating = paper.Ratings.FirstOrDefault(r => r.UserId == currentUserId.Value);
            dto.CurrentUserRating = userRating?.Score;
        }

        var ratings = await _paperRepository.GetRatingDistributionAsync(id);
        dto.RatingDistribution = new RatingDistributionDto
        {
            Star1 = ratings.Count(r => r.Score == 1),
            Star2 = ratings.Count(r => r.Score == 2),
            Star3 = ratings.Count(r => r.Score == 3),
            Star4 = ratings.Count(r => r.Score == 4),
            Star5 = ratings.Count(r => r.Score == 5)
        };

        return dto;
    }

    public async Task<PaperDto> CreateAsync(CreatePaperRequest request, Stream fileStream, string fileName, string contentType, long fileSize, Guid authorId)
    {
        if (!contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("仅接受 PDF 格式文件");

        if (fileSize > 10 * 1024 * 1024)
            throw new InvalidOperationException("文件大小不能超过 10MB");

        if (fileSize == 0)
            throw new InvalidOperationException("文件不能为空");

        var paperId = Guid.NewGuid();
        var savedPath = await _fileStorageService.SaveFileAsync(fileStream, $"{paperId}.pdf");

        var paper = new Paper
        {
            Id = paperId,
            Title = request.Title,
            Abstract = request.Abstract,
            FilePath = savedPath,
            FileSize = fileSize,
            AuthorId = authorId,
            Status = PaperStatus.Published,
            PublishedAt = DateTime.UtcNow
        };

        if (request.CategoryIds is { Count: > 0 })
        {
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category is not null)
                {
                    paper.PaperCategories.Add(new PaperCategory
                    {
                        PaperId = paperId,
                        CategoryId = categoryId
                    });
                }
            }
        }

        _paperRepository.Add(paper);

        return paper.Adapt<PaperDto>();
    }

    public async Task<PaperDto> UpdateAsync(Guid id, UpdatePaperRequest request, Guid userId)
    {
        var paper = await _paperRepository.GetByIdAsync(id);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        if (paper.AuthorId != userId)
            throw new UnauthorizedAccessException("只能编辑自己的论文");

        paper.Title = request.Title;
        paper.Abstract = request.Abstract;
        paper.UpdatedAt = DateTime.UtcNow;

        paper.PaperCategories.Clear();
        if (request.CategoryIds is { Count: > 0 })
        {
            foreach (var categoryId in request.CategoryIds)
            {
                paper.PaperCategories.Add(new PaperCategory
                {
                    PaperId = id,
                    CategoryId = categoryId
                });
            }
        }

        _paperRepository.Update(paper);

        return paper.Adapt<PaperDto>();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var paper = await _paperRepository.GetByIdAsync(id);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        if (paper.AuthorId != userId)
            throw new UnauthorizedAccessException("只能删除自己的论文");

        paper.IsDeleted = true;
        paper.UpdatedAt = DateTime.UtcNow;
        _paperRepository.Update(paper);
    }

    public async Task RetractAsync(Guid id, string reason, Guid userId)
    {
        var paper = await _paperRepository.GetByIdAsync(id);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        if (paper.AuthorId != userId)
            throw new UnauthorizedAccessException("只能撤回自己的论文");

        paper.Status = PaperStatus.Retracted;
        paper.UpdatedAt = DateTime.UtcNow;
        _paperRepository.Update(paper);
    }
}
