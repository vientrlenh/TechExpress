using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class InstallmentRepository
    {
        private readonly ApplicationDbContext _context;

        public InstallmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Installment?> FindByIdAsync(Guid installmentId)
        {
            return await _context.Installments
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == installmentId);
        }

        public async Task<Installment?> FindByIdWithTrackingAsync(Guid installmentId)
        {
            return await _context.Installments
                .AsTracking()
                .FirstOrDefaultAsync(i => i.Id == installmentId);
        }

        public async Task<List<Installment>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Installments
                .AsNoTracking()
                .Where(i => i.OrderId == orderId)
                .OrderBy(i => i.Period) // nếu có Period
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Installment> installments)
        {
            await _context.Installments.AddRangeAsync(installments);
        }

        public async Task UpdateStatusAsync(Guid installmentId, InstallmentStatus status)
        {
            // TODO
            var ins = await _context.Installments.AsTracking().FirstOrDefaultAsync(i => i.Id == installmentId);
            if (ins != null) ins.Status = status;
            //throw new NotImplementedException();
        }
    }
}
