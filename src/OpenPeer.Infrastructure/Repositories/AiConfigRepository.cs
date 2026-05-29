using Microsoft.EntityFrameworkCore;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;
using OpenPeer.Infrastructure.Data;

namespace OpenPeer.Infrastructure.Repositories;

public class AiConfigRepository : IAiConfigRepository
{
    private readonly AppDbContext _context;

    public AiConfigRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAiConfig?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserAiConfigs.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public void Add(UserAiConfig config)
    {
        _context.UserAiConfigs.Add(config);
    }

    public void Update(UserAiConfig config)
    {
        _context.UserAiConfigs.Update(config);
    }
}
