using TechExpress.Repository;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Services;

public class NotificationService
{
    private readonly UnitOfWork _unitOfWork;

    public NotificationService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Notification>> HandleGetCustomerNotificationsAsync(Guid userId)
    {
        return await _unitOfWork.NotificationRepository.FindNotificationsByUserIdAsync(userId);
    }

    public async Task<int> HandleMarkNotificationAsReadAsync(long notificationId)
    {
        return await _unitOfWork.NotificationRepository.MarkAsReadAsync(notificationId);
    }

    public async Task<int> HandleMarkAllNotificationsAsReadAsync(Guid userId)
    {
        return await _unitOfWork.NotificationRepository.MarkAllAsReadByUserIdAsync(userId);
    }
}
