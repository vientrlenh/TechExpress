using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
