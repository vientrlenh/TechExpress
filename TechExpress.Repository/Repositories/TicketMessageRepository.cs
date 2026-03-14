using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories
{
    public class TicketMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TicketMessage> AddAsync(TicketMessage message)
        {
            await _context.TicketMessages.AddAsync(message);
            return message;
        }

        public async Task<List<TicketMessage>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.TicketMessages
                .AsNoTracking()
                .Where(tm => tm.TicketId == ticketId)
                .OrderBy(tm => tm.SentAt)
                .ToListAsync();
        }

        public async Task<(List<TicketMessage> Items, int TotalCount)> FindByTicketIdPaginatedAsync(
            Guid ticketId,
            int page,
            int size)
        {
            var query = _context.TicketMessages
                .Include(m => m.Attachments)
                .Where(m => m.TicketId == ticketId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, total);
        }
    }
}
