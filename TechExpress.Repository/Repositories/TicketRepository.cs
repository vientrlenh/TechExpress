using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class TicketRepository(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
    }

    public async Task<Ticket?> FindByIdIncludeMessagesWithAttachmentsAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.Messages.OrderByDescending(m => m.SentAt))
            .ThenInclude(m => m.Attachments)
            .Include(t => t.CompletedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket?> FindByIdWithTrackingAsync(Guid id)
    {
        return await _context.Tickets.AsTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(List<Ticket> Items, int TotalCount)> FindPaginatedAsync(
        TicketStatus? status,
        bool sortAsc,
        int page,
        int size)
    {
        var query = _context.Tickets.AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var total = await query.CountAsync();

        query = sortAsc
            ? query.OrderBy(t => t.CreatedAt)
            : query.OrderByDescending(t => t.CreatedAt);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, total);
    }

}
