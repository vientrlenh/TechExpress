using TechExpress.Repository.Contexts;
using TechExpress.Repository.Models;

namespace TechExpress.Repository.Repositories;

public class NotificationRepository(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }
}
