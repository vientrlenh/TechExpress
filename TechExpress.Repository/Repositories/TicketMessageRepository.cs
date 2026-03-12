using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class TicketMessageRepository(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(TicketMessage message)
    {
        await _context.TicketMessages.AddAsync(message);
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
