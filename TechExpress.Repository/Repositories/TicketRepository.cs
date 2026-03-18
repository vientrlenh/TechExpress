using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Enums;
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

        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
        }

        public async Task<Ticket?> FindByIdAsync(Guid ticketId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi!.Order)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi!.Product)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

    public async Task<Ticket?> FindByIdFullJoinWithSplitQueryAsync(Guid id)
    {
        return await _context.Tickets
            .AsSplitQuery()
            .Include(t => t.Messages.OrderByDescending(m => m.SentAt))
            .ThenInclude(m => m.Attachments)
            .Include(t => t.CustomPC)
                .ThenInclude(pc => pc!.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
            .Include(t => t.Order)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Order)
            .Include(t => t.CompletedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

        public async Task<Ticket?> FindByIdWithTrackingAsync(Guid ticketId)
        {
            return await _context.Tickets
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId);
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

        public async Task<(List<Ticket> Items, int TotalCount)> FindPaginatedByUserIdAsync(
            Guid userId,
            TicketStatus? status,
            bool sortAsc,
            int page,
            int size)
        {
            var query = _context.Tickets.Where(t => t.UserId == userId);

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

        public async Task<(List<Ticket> Items, int TotalCount)> FindPaginatedByPhoneAsync(
            string phone,
            TicketStatus? status,
            bool sortAsc,
            int page,
            int size)
        {
            var query = _context.Tickets.Where(t => t.Phone == phone);

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
}