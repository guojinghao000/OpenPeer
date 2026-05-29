namespace OpenPeer.Application.Interfaces;

public interface IAiPaperService
{
    Task<string> GenerateLatexAsync(Guid userId, string title, List<Guid> dataIds, string prompt);
}
