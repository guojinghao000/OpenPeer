using Mapster;
using Microsoft.Extensions.Logging;
using OpenPeer.Application.DTOs.SupportingData;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Services;

public class SupportingDataService : ISupportingDataService
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp",
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/csv", "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/json", "application/zip"
    };

    private readonly ISupportingDataRepository _repository;
    private readonly IPaperRepository _paperRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SupportingDataService> _logger;

    public SupportingDataService(
        ISupportingDataRepository repository,
        IPaperRepository paperRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<SupportingDataService> logger)
    {
        _repository = repository;
        _paperRepository = paperRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SupportingDataDto> UploadAsync(Guid paperId, Guid userId, Stream fileStream, string fileName, string contentType, long fileSize, string? description)
    {
        if (!AllowedMimeTypes.Contains(contentType))
            throw new InvalidOperationException("不支持的文件类型");

        if (fileSize > 20 * 1024 * 1024)
            throw new InvalidOperationException("文件大小不能超过 20MB");

        if (fileSize == 0)
            throw new InvalidOperationException("文件不能为空");

        var paper = await _paperRepository.GetByIdAsync(paperId);
        if (paper is null || paper.IsDeleted)
            throw new KeyNotFoundException("论文不存在");

        var dataId = Guid.NewGuid();
        var ext = Path.GetExtension(fileName);
        var storedName = $"{dataId}{ext}";
        var savedPath = await _fileStorageService.SaveFileAsync(fileStream, storedName, "SupportingData");

        var entity = new SupportingData
        {
            Id = dataId,
            PaperId = paperId,
            UserId = userId,
            FileName = fileName,
            FilePath = savedPath,
            FileType = contentType,
            FileSize = fileSize,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Supporting data {DataId} uploaded for paper {PaperId} by user {UserId}", dataId, paperId, userId);

        var dto = entity.Adapt<SupportingDataDto>();
        return dto;
    }

    public async Task<List<SupportingDataDto>> GetListAsync(Guid paperId)
    {
        var items = await _repository.GetByPaperIdAsync(paperId);

        var dtos = items.Select(sd =>
        {
            var dto = sd.Adapt<SupportingDataDto>();
            dto.UserName = sd.User?.UserName ?? "未知用户";
            return dto;
        }).ToList();

        return dtos;
    }

    public async Task DeleteAsync(Guid paperId, Guid dataId, Guid userId)
    {
        var data = await _repository.GetByIdAsync(dataId);
        if (data is null || data.PaperId != paperId)
            throw new KeyNotFoundException("支撑数据不存在");

        if (data.UserId != userId)
        {
            var paper = await _paperRepository.GetByIdAsync(paperId);
            if (paper is null || paper.AuthorId != userId)
                throw new UnauthorizedAccessException("无权删除此文件");
        }

        await _fileStorageService.DeleteFileAsync(data.FilePath);
        _repository.Remove(data);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Supporting data {DataId} deleted by user {UserId}", dataId, userId);
    }
}
