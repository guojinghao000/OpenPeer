using FluentAssertions;
using Moq;
using OpenPeer.Application.Interfaces;
using OpenPeer.Application.Services;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Tests;

public class RatingServiceTests
{
    private readonly Mock<IRatingRepository> _ratingRepo;
    private readonly Mock<IPaperRepository> _paperRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly RatingService _service;

    public RatingServiceTests()
    {
        _ratingRepo = new Mock<IRatingRepository>();
        _paperRepo = new Mock<IPaperRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _service = new RatingService(_ratingRepo.Object, _paperRepo.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task SubmitOrUpdateAsync_CreatesNewRating_WhenNoneExists()
    {
        var paperId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var score = 4;

        _ratingRepo.Setup(r => r.GetByUserAndPaperAsync(userId, paperId))
            .ReturnsAsync((Rating?)null);

        _paperRepo.Setup(p => p.GetByIdAsync(paperId))
            .ReturnsAsync(new Paper { Id = paperId, Title = "Test" });

        _ratingRepo.Setup(r => r.GetAllByPaperIdAsync(paperId))
            .ReturnsAsync(new List<Rating> { new() { Score = 4 } });

        var result = await _service.SubmitOrUpdateAsync(paperId, userId, score);

        result.Should().NotBeNull();
        _ratingRepo.Verify(r => r.Add(It.IsAny<Rating>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitOrUpdateAsync_UpdatesExistingRating_WhenAlreadyExists()
    {
        var paperId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingRating = new Rating
        {
            Id = Guid.NewGuid(),
            PaperId = paperId,
            UserId = userId,
            Score = 3
        };

        _ratingRepo.Setup(r => r.GetByUserAndPaperAsync(userId, paperId))
            .ReturnsAsync(existingRating);

        _paperRepo.Setup(p => p.GetByIdAsync(paperId))
            .ReturnsAsync(new Paper { Id = paperId, Title = "Test" });

        _ratingRepo.Setup(r => r.GetAllByPaperIdAsync(paperId))
            .ReturnsAsync(new List<Rating> { new() { Score = 5 } });

        var result = await _service.SubmitOrUpdateAsync(paperId, userId, 5);

        existingRating.Score.Should().Be(5);
        _ratingRepo.Verify(r => r.Update(It.IsAny<Rating>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitOrUpdateAsync_Throws_WhenPaperNotFound()
    {
        var paperId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _paperRepo.Setup(p => p.GetByIdAsync(paperId))
            .ReturnsAsync((Paper?)null);

        var act = async () => await _service.SubmitOrUpdateAsync(paperId, userId, 3);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetListAsync_ReturnsPagedRatings()
    {
        var paperId = Guid.NewGuid();
        var ratings = new List<Rating>
        {
            new() { Id = Guid.NewGuid(), PaperId = paperId, Score = 5, UserId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), PaperId = paperId, Score = 4, UserId = Guid.NewGuid() },
        };

        _ratingRepo.Setup(r => r.GetPagedByPaperIdAsync(paperId, 1, 20))
            .ReturnsAsync((ratings, 2));

        _ratingRepo.Setup(r => r.GetAllByPaperIdAsync(paperId))
            .ReturnsAsync(ratings);

        var result = await _service.GetListAsync(paperId, 1, 20);

        result.Total.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Distribution.Should().NotBeNull();
        result.Distribution!.Star5.Should().Be(1);
        result.Distribution.Star4.Should().Be(1);
    }
}
