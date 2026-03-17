using Microsoft.EntityFrameworkCore;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class NotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> FindNotificationsByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> FindByIdAsync(long notificationId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task<Notification?> FindByIdWithTrackingAsync(long notificationId)
    {
        return await _context.Notifications
            .AsTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }

    public async Task<int> MarkAsReadAsync(long notificationId)
    {
        return await _context.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTimeOffset.Now)
            );
    }

    public async Task<int> MarkAllAsReadByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTimeOffset.Now)
            );
    }
}
