using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class PromotionRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionRepository(ApplicationDbContext context)
    {
        _context = context; 
    }

    public async Task<Promotion?> FindByCodeAsync(string code)
    {
        return await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _context.Promotions.AnyAsync(p => p.Code == code);
    }

    public async Task AddAsync(Promotion promotion)
    {
        await _context.Promotions.AddAsync(promotion);
    }

    public async Task<Promotion?> FindByIdIncludeRequiredProductsIncludeFreeProductsIncludeAppliedProductsWithSplitQueryAsync(Guid id)
    {
        return await _context.Promotions
            .Include(p => p.RequiredProducts)
            .Include(p => p.FreeProducts)
            .Include(p => p.AppliedProducts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Promotion>> FindActiveAutoApplyAsync(DateTimeOffset now)
    {
        return await _context.Promotions
            .Include(p => p.RequiredProducts)
            .Include(p => p.FreeProducts)
            .Include(p => p.AppliedProducts)
            .AsSplitQuery()
            .Where(p => p.Code == null && p.StartDate <= now && p.EndDate > now && p.IsActive).ToListAsync();
    }

    public async Task<List<Promotion>> FindActiveNonAutoApplyAsync(List<string> codes, DateTimeOffset now)
    {
        return await _context.Promotions
            .Include(p => p.RequiredProducts)
            .Include(p => p.FreeProducts)
            .Include(p => p.AppliedProducts)
            .AsSplitQuery()
            .Where(p => !string.IsNullOrEmpty(p.Code) && codes.Contains(p.Code) && p.StartDate <= now && p.EndDate > now && p.IsActive).ToListAsync();
    } 

    public async Task<int> IncrementUsageCountIfMaxUsageNotExceed(Guid id)
    {
        return await _context.Promotions
            .Where(p => p.Id == id && (p.MaxUsageCount == null || p.UsageCount < p.MaxUsageCount))
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.UsageCount, x => x.UsageCount + 1));
    }
}
