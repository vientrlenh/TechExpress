using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class PaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> FindByIdAsync(long paymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<List<Payment>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentDate) // nếu có
                .ToListAsync();
        }

        public async Task<List<Payment>> GetByInstallmentIdAsync(Guid installmentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.InstallmentId == installmentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        /// <summary>
        /// Dùng cho idempotency (gateway transaction ref / sessionId / orderCode...)
        /// Bạn cần có cột tương ứng trong Payment (ví dụ GatewayRef) thì mới implement.
        /// </summary>
        public async Task<bool> ExistsByGatewayRefAsync(string gatewayRef)
        {
            // TODO: nếu Payment có field GatewayRef hoặc TransactionCode
            // return await _context.Payments.AnyAsync(p => p.GatewayRef == gatewayRef);
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }


        public async Task<decimal> SumSuccessAmountByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Success)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> SumSuccessAmountByInstallmentIdAsync(Guid installmentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.InstallmentId == installmentId && p.Status == PaymentStatus.Success)
                .SumAsync(p => p.Amount);
        }




    }
}
