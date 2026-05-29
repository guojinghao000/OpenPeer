using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Data;

namespace OpenPeer.Infrastructure.Repositories;

public class SupportingDataRepository : ISupportingDataRepository
{
    private readonly AppDbContext _context;

    public SupportingDataRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupportingData>> GetByPaperIdAsync(Guid paperId)
    {
        return await _context.SupportingData
            .Include(sd => sd.User)
            .Where(sd => sd.PaperId == paperId)
            .OrderByDescending(sd => sd.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportingData?> GetByIdAsync(Guid id)
    {
        return await _context.SupportingData
            .Include(sd => sd.User)
            .FirstOrDefaultAsync(sd => sd.Id == id);
    }

    public void Add(SupportingData data)
    {
        _context.SupportingData.Add(data);
    }

    public void Remove(SupportingData data)
    {
        _context.SupportingData.Remove(data);
    }
}
