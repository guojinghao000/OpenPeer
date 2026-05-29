using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Infrastructure.AI;

public class AiApiClient : IAiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiApiClient> _logger;

    public AiApiClient(HttpClient httpClient, ILogger<AiApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateLatexAsync(string provider, string apiKey, string model, string systemPrompt, string userMessage)
    {
        var baseUrl = provider.ToLowerInvariant() switch
        {
            "openai" => "https://api.openai.com/v1/chat/completions",
            "deepseek" => "https://api.deepseek.com/v1/chat/completions",
            "anthropic" => "https://api.anthropic.com/v1/messages",
            _ => throw new InvalidOperationException($"未知 AI 服务商: {provider}")
        };

        if (provider.Equals("anthropic", StringComparison.OrdinalIgnoreCase))
        {
            return await CallAnthropicAsync(baseUrl, apiKey, model, systemPrompt, userMessage);
        }

        return await CallOpenAiCompatibleAsync(baseUrl, apiKey, model, systemPrompt, userMessage);
    }

    private async Task<string> CallOpenAiCompatibleAsync(string url, string apiKey, string model, string systemPrompt, string userMessage)
    {
        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            max_tokens = 8192,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = content;

        _logger.LogInformation("Calling AI API at {Url} with model {Model}", UrlMasked(url), model);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AI API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"AI API 调用失败: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var choice = doc.RootElement.GetProperty("choices")[0];
        var message = choice.GetProperty("message");
        var latex = message.GetProperty("content").GetString();

        return latex ?? string.Empty;
    }

    private async Task<string> CallAnthropicAsync(string url, string apiKey, string model, string systemPrompt, string userMessage)
    {
        var requestBody = new
        {
            model,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            },
            max_tokens = 8192,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = content;

        _logger.LogInformation("Calling Anthropic API with model {Model}", model);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"AI API 调用失败: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var latex = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();

        return latex ?? string.Empty;
    }

    private static string UrlMasked(string url)
    {
        var uri = new Uri(url);
        return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
    }
}
