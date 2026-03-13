using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class TicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket?> FindByIdAsync(Guid ticketId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<Ticket?> FindByIdWithTrackingAsync(Guid ticketId)
        {
            return await _context.Tickets
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }
    }
}
