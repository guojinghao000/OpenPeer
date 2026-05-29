using System.Text;
using Microsoft.Extensions.Logging;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Application.Services;

public class AiPaperService : IAiPaperService
{
    private readonly IAiConfigRepository _aiConfigRepository;
    private readonly ISupportingDataRepository _dataRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEncryptionService _encryption;
    private readonly IAiApiClient _aiApiClient;
    private readonly ILogger<AiPaperService> _logger;

    public AiPaperService(
        IAiConfigRepository aiConfigRepository,
        ISupportingDataRepository dataRepository,
        IFileStorageService fileStorageService,
        IEncryptionService encryption,
        IAiApiClient aiApiClient,
        ILogger<AiPaperService> logger)
    {
        _aiConfigRepository = aiConfigRepository;
        _dataRepository = dataRepository;
        _fileStorageService = fileStorageService;
        _encryption = encryption;
        _aiApiClient = aiApiClient;
        _logger = logger;
    }

    public async Task<string> GenerateLatexAsync(Guid userId, string title, List<Guid> dataIds, string prompt)
    {
        var config = await _aiConfigRepository.GetByUserIdAsync(userId);
        if (config is null || string.IsNullOrEmpty(config.ApiKey))
            throw new InvalidOperationException("请先配置 AI API");

        var apiKey = _encryption.Decrypt(config.ApiKey);

        var dataContents = new StringBuilder();
        foreach (var dataId in dataIds)
        {
            var data = await _dataRepository.GetByIdAsync(dataId);
            if (data is null) continue;

            var stream = await _fileStorageService.GetFileAsync(data.FilePath);
            if (stream is null) continue;

            dataContents.AppendLine($"### 文件名: {data.FileName} ({data.FileType}, {data.FileSize} 字节)");
            if (data.Description is not null)
                dataContents.AppendLine($"描述: {data.Description}");

            if (data.FileType.StartsWith("text/") ||
                data.FileType == "application/json" ||
                data.FileType == "text/csv")
            {
                using var reader = new StreamReader(stream);
                var text = await reader.ReadToEndAsync();
                if (text.Length > 20000)
                    text = text[..20000] + "\n... [内容过长，已截断]";
                dataContents.AppendLine("内容:");
                dataContents.AppendLine(text);
            }
            else
            {
                dataContents.AppendLine("(二进制文件，仅提供元数据)");
            }
            dataContents.AppendLine();
        }

        var systemPrompt = @"你是一位学术论文写作助手。请根据用户提供的研究数据，生成一篇完整的 LaTeX 格式的学术论文。

要求:
1. 使用标准的 article 文档类
2. 包含标题、作者、摘要、引言、方法、结果、结论、参考文献等完整结构
3. 只输出 LaTeX 源码，不要有任何额外说明
4. 使用 \documentclass[12pt,a4paper]{article}
5. 支持中文使用 ctex 宏包
6. 如果需要引用数据表格，使用 tabular 环境
7. 参考文献使用 thebibliography 环境";

        var userMessage = $"论文标题: {title}\n\n{prompt}\n\n参考数据:\n{dataContents}";

        _logger.LogInformation("Generating LaTeX paper '{Title}' for user {UserId} with {DataCount} data files",
            title, userId, dataIds.Count);

        var latex = await _aiApiClient.GenerateLatexAsync(config.Provider, apiKey, config.Model, systemPrompt, userMessage);

        return latex;
    }
}
