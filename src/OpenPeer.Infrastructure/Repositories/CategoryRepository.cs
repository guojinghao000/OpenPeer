using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Data;

namespace OpenPeer.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public void Add(Category category)
    {
        _context.Categories.Add(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is not null)
            _context.Categories.Remove(category);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
