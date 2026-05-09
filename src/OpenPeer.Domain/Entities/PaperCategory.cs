namespace OpenPeer.Domain.Entities;

public class PaperCategory
{
    public Guid PaperId { get; set; }
    public Guid CategoryId { get; set; }

    public Paper Paper { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
