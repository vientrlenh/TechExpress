using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class OrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> FindByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> FindByIdWithTrackingAsync(Guid orderId)
        {
            return await _context.Orders
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> FindByIdIncludeDetailsAsync(Guid orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> FindByIdIncludeItemsWithProductAsync(Guid orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdatePaidTypeAsync(Guid orderId, PaidType paidType)
        {
            var order = await _context.Orders
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
                order.PaidType = paidType;
        }

        public Task<int> DeleteExpiredUnpaidOrdersAsync(
            DateTimeOffset now,
            CancellationToken ct = default)
        {
            var cutoff = now.AddMinutes(-15);

            return _context.Orders
                .Where(o =>
                    o.Status == OrderStatus.Pending &&
                    //o.PaidType == PaidType.Full &&
                    o.OrderDate <= cutoff &&
                    !_context.Payments.Any(p => p.OrderId == o.Id)
                )
                .ExecuteDeleteAsync(ct);
                }
        

        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task<(List<Order> Orders, int TotalCount)> FindOrdersPagedSortByOrderDateAsync(
            int page, int pageSize, bool isDescending, string? search, OrderStatus? status)
        {
            var query = BuildFilteredQuery(search, status);

            query = isDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }

        public async Task<(List<Order> Orders, int TotalCount)> FindOrdersPagedSortByTotalPriceAsync(
            int page, int pageSize, bool isDescending, string? search, OrderStatus? status)
        {
            var query = BuildFilteredQuery(search, status);

            query = isDescending ? query.OrderByDescending(o => o.TotalPrice) : query.OrderBy(o => o.TotalPrice);

            return await ExecutePagedQueryAsync(query, page, pageSize);
        }

        private IQueryable<Order> BuildFilteredQuery(string? search, OrderStatus? status)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(o =>
                    (o.ReceiverEmail != null && o.ReceiverEmail.ToLower().Contains(s)) ||
                    (o.ReceiverFullName != null && o.ReceiverFullName.ToLower().Contains(s)) ||
                    o.TrackingPhone.ToLower().Contains(s));
            }

            return query;
        }

        private async Task<(List<Order> Orders, int TotalCount)> ExecutePagedQueryAsync(
            IQueryable<Order> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }
        
        // Quan trọng: Phải include cả Items và Product để lấy tên sản phẩm trong OrderItem
        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
