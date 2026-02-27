using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<(List<Order> Items, int TotalCount)> GetPagedByUserIdAsync(
            Guid userId,
            int page,
            int pageSize,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            bool sortAsc,
            CancellationToken ct = default)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            if (orderStatus.HasValue)
                query = query.Where(o => o.Status == orderStatus.Value);

            if (paymentStatus.HasValue)
                query = query.Where(o => o.Payments.Any(p => p.Status == paymentStatus.Value));

            var totalCount = await query.CountAsync(ct);

            query = sortAsc
                ? query.OrderBy(o => o.OrderDate)
                : query.OrderByDescending(o => o.OrderDate);

            var items = await query
                .Include(o => o.Payments)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Order?> FindByIdForCustomerAsync(Guid orderId, Guid userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
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
