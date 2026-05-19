using Mapster;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.DTOs.Ratings;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IPaperRepository _paperRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RatingService(IRatingRepository ratingRepository, IPaperRepository paperRepository, IUnitOfWork unitOfWork)
    {
        _ratingRepository = ratingRepository;
        _paperRepository = paperRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RatingDto> SubmitOrUpdateAsync(Guid paperId, Guid userId, int score)
    {
        var paper = await _paperRepository.GetByIdAsync(paperId);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        var existing = await _ratingRepository.GetByUserAndPaperAsync(userId, paperId);

        if (existing is not null)
        {
            existing.Score = score;
            existing.UpdatedAt = DateTime.UtcNow;
            _ratingRepository.Update(existing);
        }
        else
        {
            existing = new Rating
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                UserId = userId,
                Score = score,
                CreatedAt = DateTime.UtcNow
            };
            _ratingRepository.Add(existing);
        }

        await RecalculateAverageRatingAsync(paperId, paper);
        await _unitOfWork.SaveChangesAsync();

        var dto = existing.Adapt<RatingDto>();
        dto.User = new RatingUserDto { Id = userId };
        return dto;
    }

    public async Task<RatingListResponse> GetListAsync(Guid paperId, int page, int pageSize)
    {
        var (items, total) = await _ratingRepository.GetPagedByPaperIdAsync(paperId, page, pageSize);

        var dtos = items.Adapt<List<RatingDto>>();

        var allRatings = await _ratingRepository.GetAllByPaperIdAsync(paperId);
        var distribution = new RatingDistributionDto
        {
            Star1 = allRatings.Count(r => r.Score == 1),
            Star2 = allRatings.Count(r => r.Score == 2),
            Star3 = allRatings.Count(r => r.Score == 3),
            Star4 = allRatings.Count(r => r.Score == 4),
            Star5 = allRatings.Count(r => r.Score == 5)
        };

        return new RatingListResponse
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Distribution = distribution
        };
    }

    public async Task DeleteAsync(Guid paperId, Guid userId)
    {
        var paper = await _paperRepository.GetByIdAsync(paperId);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        var rating = await _ratingRepository.GetByUserAndPaperAsync(userId, paperId);
        if (rating is null)
            throw new KeyNotFoundException("评分不存在");

        _ratingRepository.Remove(rating);
        await RecalculateAverageRatingAsync(paperId, paper);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task RecalculateAverageRatingAsync(Guid paperId, Paper paper)
    {
        var allRatings = await _ratingRepository.GetAllByPaperIdAsync(paperId);
        if (allRatings.Count > 0)
        {
            paper.AverageRating = allRatings.Average(r => r.Score);
            paper.RatingCount = allRatings.Count;
        }
        else
        {
            paper.AverageRating = 0;
            paper.RatingCount = 0;
        }
        _paperRepository.Update(paper);
    }
}
