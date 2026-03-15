using System;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
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
    public async Task AddRangeAsync(IEnumerable<PromotionUsage> usages)
    {
        await _context.PromotionUsages.AddRangeAsync(usages);
    }

    public async Task<List<PromotionUsage>> GetByOrderIdIncludePromotionAsync(Guid orderId)
    {
        return await _context.PromotionUsages
            .Include(pu => pu.Promotion) // Bắt buộc Include để lấy Code và Name
            .Where(pu => pu.OrderId == orderId)
            .ToListAsync();
    }   
  
    public async Task<int> DeletePromotionUsagesOnExpiredOrders(DateTimeOffset expiration)
    {
        return await _context.PromotionUsages
            .Where(p => _context.Orders.Any(o =>
                o.Id == p.OrderId &&
                o.Status == OrderStatus.Pending &&
                o.OrderDate <= expiration &&
                !_context.Payments.Any(pay => pay.OrderId == o.Id && pay.Status == PaymentStatus.Success)))
            .ExecuteDeleteAsync();
    }

    // == Đếm số lần sử dụng của một người dùng cho nhiều khuyến mãi ==
    public async Task<Dictionary<Guid, int>> CountByPromotionIdsAndUserIdAsync(List<Guid> promoIds, Guid userId)
    {
        return await _context.PromotionUsages
            .Where(u => promoIds.Contains(u.PromotionId) && u.UserId == userId)
            .GroupBy(u => u.PromotionId)
            .Select(g => new
            {
                PromotionId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.PromotionId, x => x.Count);
    }

    //== Đếm số lần sử dụng của một người dùng cho nhiều khuyến mãi dựa trên số điện thoại ==
    public async Task<Dictionary<Guid, int>> CountByPromotionIdsAndPhoneAsync(List<Guid> promoIds, string phone)
    {
        return await _context.PromotionUsages
            .Where(u => promoIds.Contains(u.PromotionId) && u.Phone == phone)
            .GroupBy(u => u.PromotionId)
            .Select(g => new
            {
                PromotionId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.PromotionId, x => x.Count);
    }
}
