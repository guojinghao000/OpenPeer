namespace OpenPeer.Application.Interfaces;

public interface IAiApiClient
{
    Task<string> GenerateLatexAsync(string provider, string apiKey, string model, string systemPrompt, string userMessage);
}
