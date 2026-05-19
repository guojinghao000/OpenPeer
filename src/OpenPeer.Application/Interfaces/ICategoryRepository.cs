using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    void Add(Category category);
    void Update(Category category);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
