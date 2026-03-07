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

    public async Task<List<CustomPC>> FindByUserIdIncludeItemsWithSplitQueryAsync(Guid userId)
    {
        return await _context.CustomPCs.Include(c => c.Items).Where(c => c.UserId == userId).ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await _context.CustomPCs.CountAsync(c => c.UserId == userId);
    }

    public async Task<CustomPC?> FindByIdWithTrackingAsync(Guid id)
    {
        return await _context.CustomPCs.AsTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public void Remove(CustomPC customPC)
    {
        _context.CustomPCs.Remove(customPC);
    }
}
