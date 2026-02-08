using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Repositories
{
    public class OrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<int> DeleteExpiredUnpaidOrdersAsync(
            DateTimeOffset now,
            CancellationToken ct = default)
        {
            var cutoff = now.AddMinutes(-15);

            return _context.Orders
                .Where(o =>
                    o.Status == OrderStatus.Pending &&
                        o.PaidType == PaidType.Full &&          

                    o.OrderDate <= cutoff &&
                    !_context.Payments.Any(p =>
                        p.OrderId == o.Id 
                        //&&
                        //p.Status == PaymentStatus.Success
                    )
                )
                .ExecuteDeleteAsync(ct);
        }
    }
}
