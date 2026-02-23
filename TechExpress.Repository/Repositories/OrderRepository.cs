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
    }
}
