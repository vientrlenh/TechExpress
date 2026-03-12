using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class CustomPCRepository
{
    private readonly ApplicationDbContext _context;

    public CustomPCRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CustomPC customPC)
    {
        await _context.CustomPCs.AddAsync(customPC);
    }

    public async Task<CustomPC?> FindByIdIncludeItemsAsync(Guid id)
    {
        return await _context.CustomPCs.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CustomPC?> FindByIdIncludeItemsWithTrackingAsync(Guid id)
    {
        return await _context.CustomPCs.AsTracking().Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CustomPC?> FindByIdIncludeItemsThenIncludeProductWithSplitQueryAsync(Guid id)
    {
        return await _context.CustomPCs
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }       

    public async Task<List<CustomPC>> FindByUserIdIncludeItemsThenIncludeProductWithSplitQueryAsync(Guid userId)
    {
        return await _context.CustomPCs
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Where(c => c.UserId == userId)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await _context.CustomPCs.CountAsync(c => c.UserId == userId);
    }

    public async Task<int> CountBySessionIdAsync(string sessionId)
    {
        return await _context.CustomPCs.CountAsync(c => c.SessionId == sessionId);
    }

    public async Task<List<CustomPC>> FindBySessionIdIncludeItemsThenIncludeProductWithSplitQueryAsync(string sessionId)
    {
        return await _context.CustomPCs
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Where(c => c.SessionId == sessionId)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<CustomPC?> FindByIdWithTrackingAsync(Guid id)
    {
        return await _context.CustomPCs.AsTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CustomPC?> FindByIdIncludeItemsWithTrackingBySessionAsync(Guid id, string sessionId)
    {
        return await _context.CustomPCs
            .AsTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && c.SessionId == sessionId);
    }

    public void Remove(CustomPC customPC)
    {
        _context.CustomPCs.Remove(customPC);
    }
}
