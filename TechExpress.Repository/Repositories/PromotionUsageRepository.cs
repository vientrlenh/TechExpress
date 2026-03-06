using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class PromotionUsageRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionUsageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountByPromotionAndUserIdAsync(Guid promotionId, Guid userId)
    {
        return await _context.PromotionUsages.CountAsync(p => p.PromotionId == promotionId && p.UserId == userId);
    }

    public async Task<int> CountByPromotionAndPhoneAsync(Guid promotionId, string phone)
    {
        return await _context.PromotionUsages.CountAsync(p => p.PromotionId == promotionId && p.Phone == phone);
    }
}
