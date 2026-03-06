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
        return await _context.Promotions.FirstOrDefaultAsync(p => !string.IsNullOrEmpty(p.Code) && p.Code == code);
    }

    // == Tìm kiếm khuyến mãi theo ID và bao gồm các sản phẩm đã áp dụng ==
    public async Task<Promotion?> FindByIdIncludeAppliedProductsAsync(Guid id)
    {
        return await _context.Promotions
            .AsNoTracking() // EF Core sẽ không theo dõi thay đổi, giúp tiết kiệm CPU và RAM
            .Include(p => p.AppliedProducts)
            .AsSplitQuery() // Tách lệnh truy vấn nếu danh sách sản phẩm đi kèm quá lớn
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // TechExpress.Repository/Repositories/PromotionRepository.cs

    public async Task<(List<Promotion> Promotions, int TotalCount)> FindPromotionsPagedAsync(
        string? search,
        string? status, // "Active", "Inactive", "Expired", "Upcoming"
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        string sortBy,
        bool isDescending,
        int page,
        int pageSize)
    {
        var query = _context.Promotions.AsNoTracking().AsQueryable();
        var now = DateTimeOffset.Now;

        // 1. Search theo Name hoặc Code
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || (p.Code != null && p.Code.Contains(s)));
        }

        // 2. Filter Status (Logic tính toán động)
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = status.ToLower() switch
            {
                "active" => query.Where(p => p.IsActive && now >= p.StartDate && now <= p.EndDate),
                "inactive" => query.Where(p => !p.IsActive),
                "expired" => query.Where(p => now > p.EndDate),
                "upcoming" => query.Where(p => p.IsActive && now < p.StartDate),
                _ => query
            };
        }

        // 3. Filter Date Range
        if (fromDate.HasValue) query = query.Where(p => p.StartDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.EndDate <= toDate.Value);

        // 4. Sorting (Chỉ cho phép Name và Code)
        query = sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "code" => isDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
            _ => query.OrderByDescending(p => p.CreatedAt) // Mặc định vẫn nên có Sort theo ngày tạo
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}
