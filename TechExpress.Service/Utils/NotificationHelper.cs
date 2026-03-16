using Microsoft.AspNetCore.SignalR;
using TechExpress.Repository;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Constants;
using TechExpress.Service.Hubs;

namespace TechExpress.Service.Utils;

public class NotificationHelper
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _notificationHubContext;

    public NotificationHelper(UnitOfWork unitOfWork, IHubContext<NotificationHub> notificationHubContext)
    {
        _unitOfWork = unitOfWork;
        _notificationHubContext = notificationHubContext;
    }

    /// <summary>
    /// T?o notification cho Order status change
    /// </summary>
    public async Task CreateOrderNotificationAsync(Guid userId, Guid orderId, OrderStatus status)
    {
        var (title, message) = GetOrderNotificationMessage(status);
        
        var notification = new Notification
        {
            UserId = userId,
            Type = GetOrderNotificationType(status),
            Title = title,
            Message = message,
            ReferenceId = orderId,
            ReferenceType = NotificationReferenceType.Order,
            IsRead = false
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate);
    }

    /// <summary>
    /// T?o notification cho Payment status change
    /// </summary>
    public async Task CreatePaymentNotificationAsync(Guid userId, Guid orderId, PaymentStatus paymentStatus)
    {
        var notificationType = paymentStatus == PaymentStatus.Success 
            ? NotificationType.PaymentSuccess 
            : NotificationType.PaymentFailed;

        var message = paymentStatus == PaymentStatus.Success
            ? "Thanh toán của bạn đã được xác nhận thành công"
            : "Thanh toán của bạn thất bại, vui lòng thử lại";

        var notification = new Notification
        {
            UserId = userId,
            Type = notificationType,
            Title = paymentStatus == PaymentStatus.Success ? "Thanh toán thành công" : "Thanh toán thất bại",
            Message = message,
            ReferenceId = orderId,
            ReferenceType = NotificationReferenceType.Order,
            IsRead = false
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate);
    }

    /// <summary>
    /// Tạo notification cho Stock Alert (khi sản phẩm hết hàng)
    /// Gửi cho tất cả admin users
    /// </summary>
    public async Task CreateStockAlertNotificationAsync(Guid productId, string productName)
    {
        // Lấy tất cả admin users
        var admins = await _unitOfWork.UserRepository.FindAdminUsersAsync();
        if (!admins.Any())
            return;

        foreach (var admin in admins)
        {
            var notification = new Notification
            {
                UserId = admin.Id,
                Type = NotificationType.StockAlert,
                Title = "Sản phẩm hết hàng",
                Message = $"Sản phẩm '{productName}' đã hết hàng. Vui lòng kiểm tra và bổ sung kho",
                ReferenceId = productId,
                ReferenceType = NotificationReferenceType.Product,
                IsRead = false
            };

            await _unitOfWork.NotificationRepository.AddAsync(notification);
            await _notificationHubContext.Clients.User(admin.Id.ToString())
                .SendAsync(SignalRMessageConstant.NotificationListUpdate);
        }
    }

    /// <summary>
    /// T?o notification cho Review Response
    /// </summary>
    public async Task CreateReviewResponseNotificationAsync(Guid userId, Guid reviewId, string productName)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.ReviewResponse,
            Title = "Có phản hồi cho đánh giá của bạn",
            Message = $"Người bán đã phản hồi cho đánh giá của bạn về sản phẩm {productName}",
            ReferenceId = reviewId,
            ReferenceType = NotificationReferenceType.Review,
            IsRead = false
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate);
    }

    /// <summary>
    /// Tạo notification cho product owner khi có review mới
    /// </summary>
    public async Task CreateNewReviewNotificationAsync(Guid productOwnerId, Guid productId, string productName, string reviewerName)
    {
        var notification = new Notification
        {
            UserId = productOwnerId,
            Type = NotificationType.ReviewResponse,
            Title = "Có đánh giá mới",
            Message = $"{reviewerName} đã đánh giá sản phẩm {productName} của bạn",
            ReferenceId = productId,
            ReferenceType = NotificationReferenceType.Product,
            IsRead = false
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _notificationHubContext.Clients.User(productOwnerId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate);
    }

    /// <summary>
    /// Tạo notification hệ thống chung
    /// </summary>
    public async Task CreateSystemNotificationAsync(Guid userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.System,
            Title = title,
            Message = message,
            ReferenceId = null,
            ReferenceType = null,
            IsRead = false
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate);
    }

    /// <summary>
    /// Tạo notification cho Promotion Alert - gửi cho tất cả customer
    /// </summary>
    public async Task CreatePromotionNotificationForAllCustomersAsync(Guid promotionId, string promotionCode, string promotionName)
    {
        // Lấy tất cả customer users
        var customers = await _unitOfWork.UserRepository.FindAllCustomersAsync();
        if (!customers.Any())
            return;

        foreach (var customer in customers)
        {
            await CreatePromotionNotificationAsync(customer.Id, promotionId, promotionCode, promotionName);
        }
    }

    /// <summary>
    /// Tạo notification cho Promotion Alert
    /// </summary>
    public async Task CreatePromotionNotificationAsync(Guid userId, Guid promotionId, string promotionCode, string promotionName)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.PromotionAlert,
            Title = "Khuyến mãi mới",
            Message = $"Bạn có thể dùng mã '{promotionCode}' để nhận {promotionName}",
            ReferenceId = promotionId,
            ReferenceType = NotificationReferenceType.Promotion,
            IsRead = false
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate);
    }

    private NotificationType GetOrderNotificationType(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => NotificationType.OrderPlaced,
            OrderStatus.Confirmed => NotificationType.OrderPlaced,
            OrderStatus.Processing => NotificationType.OrderPlaced,
            OrderStatus.ReadyForPickup => NotificationType.OrderShipped,
            OrderStatus.Shipping => NotificationType.OrderShipped,
            OrderStatus.Delivered => NotificationType.OrderDelievered,
            OrderStatus.PickedUp => NotificationType.OrderDelievered,
            OrderStatus.Completed => NotificationType.OrderDelievered,
            OrderStatus.Installing => NotificationType.OrderDelievered,
            OrderStatus.Canceled => NotificationType.OrderCancelled,
            OrderStatus.Refunded => NotificationType.OrderCancelled,
            OrderStatus.Expired => NotificationType.OrderCancelled,
            _ => NotificationType.System
        };
    }

    private (string Title, string Message) GetOrderNotificationMessage(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => ("Đơn hàng chờ xác nhận", "Đơn hàng của bạn đang chờ xác nhận từ cửa hàng"),
            OrderStatus.Confirmed => ("Đơn hàng đã được xác nhận", "Đơn hàng của bạn đã được xác nhận thành công"),
            OrderStatus.Processing => ("Đơn hàng đang xử lý", "Cửa hàng đang chuẩn bị đơn hàng của bạn"),
            OrderStatus.ReadyForPickup => ("Đơn hàng sẵn sàng lấy", "Đơn hàng của bạn đã sẵn sàng để lấy"),
            OrderStatus.Shipping => ("Đơn hàng đang vận chuyển", "Đơn hàng của bạn đang trên đường giao đến"),
            OrderStatus.Delivered => ("Đơn hàng đã được giao", "Đơn hàng của bạn đã được giao thành công"),
            OrderStatus.PickedUp => ("Đơn hàng đã được nhận", "Bạn đã nhận đơn hàng thành công"),
            OrderStatus.Completed => ("Đơn hàng hoàn thành", "Đơn hàng của bạn đã hoàn thành"),
            OrderStatus.Installing => ("Đơn hàng đang lắp đặt", "Cửa hàng đang lắp đặt sản phẩm của bạn"),
            OrderStatus.Canceled => ("Đơn hàng đã bị hủy", "Đơn hàng của bạn đã bị hủy"),
            OrderStatus.Refunded => ("Hoàn tiền thành công", "Tiền hoàn lại cho bạn đã được xử lý"),
            OrderStatus.Expired => ("Đơn hàng hết hạn", "Đơn hàng của bạn đã hết hạn"),
            _ => ("Cập nhật đơn hàng", "Đơn hàng của bạn có cập nhật mới")
        };
    }
}
