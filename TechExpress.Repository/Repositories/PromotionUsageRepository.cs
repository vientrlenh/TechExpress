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

    // Bổ sung phương thức AddAsync để lưu lịch sử sử dụng khuyến mãi
    public async Task AddAsync(PromotionUsage usage)
    {
        await _context.PromotionUsages.AddAsync(usage);
    }
    public async Task<List<PromotionUsage>> GetByOrderIdIncludePromotionAsync(Guid orderId)
    {
        return await _context.PromotionUsages
            .Include(pu => pu.Promotion) // Bắt buộc Include để lấy Code và Name
            .Where(pu => pu.OrderId == orderId)
            .ToListAsync();
    }   
}
