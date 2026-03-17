using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;
using TechExpress.Service.Constants;

namespace TechExpress.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly ServiceProviders _serviceProvider;
    private readonly UserContext _userContext;
    private readonly IHubContext<NotificationHub> _notificationHubContext;

    public NotificationController(ServiceProviders serviceProvider, UserContext userContext, IHubContext<NotificationHub> notificationHubContext)
    {
        _serviceProvider = serviceProvider;
        _userContext = userContext;
        _notificationHubContext = notificationHubContext;
    }

    /// <summary>
    /// Lấy tất cả thông báo của khách hàng hiện tại (tất cả đã đọc)
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllNotifications()
    {
        var userId = _userContext.GetCurrentAuthenticatedUserId();

        var notifications = await _serviceProvider.NotificationService
            .HandleGetCustomerNotificationsAsync(userId);

        // Tất cả notifications đã đọc
        await _serviceProvider.NotificationService
            .HandleMarkAllNotificationsAsReadAsync(userId);

        await _serviceProvider.UnitOfWork.SaveChangesAsync();

        var response = ResponseMapper.MapToNotificationResponseListFromNotificationList(notifications);

        // Gửi danh sách notification mới nhất qua SignalR cho user hiện tại    
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync(SignalRMessageConstant.NotificationListUpdate, response);

        return Ok(ApiResponse<List<NotificationResponse>>.OkResponse(response));
    }
}
