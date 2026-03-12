using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Service;
using TechExpress.Service.Constants;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TicketController(
    ServiceProviders serviceProviders,
    UserContext userContext,
    IHubContext<NotificationHub> notificationHub) : ControllerBase
{
    private readonly ServiceProviders _serviceProviders = serviceProviders;
    private readonly UserContext _userContext = userContext;
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;

    // ── POST /api/tickets/custom-pc-build  (guest + logged-in) ──────────
    [AllowAnonymous]
    [HttpPost("custom-pc-build")]
    public async Task<IActionResult> CreateCustomPCBuildTicket([FromBody] CreateCustomPCBuildTicketRequest request)
    {
        var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
        var ticket = await _serviceProviders.TicketService.HandleCreateCustomPCBuildTicket(
            userIdStr,
            request.FullName?.Trim(),
            request.Phone?.Trim(),
            request.Title.Trim(),
            request.Message.Trim(),
            request.CustomPCId,
            request.Attachments
        );
        var response = ResponseMapper.MapToTicketResponse(ticket);
        return CreatedAtAction(nameof(CreateCustomPCBuildTicket), ApiResponse<TicketResponse>.CreatedResponse(response));
    }

    // ── POST /api/tickets  (customer only) ───────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var userId = _userContext.GetCurrentAuthenticatedUserId();
        var ticket = await _serviceProviders.TicketService.HandleCreateTicket(
            userId,
            request.Title.Trim(),
            request.Message.Trim(),
            request.Type,
            request.CustomPCId,
            request.Attachments
        );
        var response = ResponseMapper.MapToTicketResponse(ticket);
        return CreatedAtAction(nameof(CreateTicket), ApiResponse<TicketResponse>.CreatedResponse(response));
    }

    // ── GET /api/tickets/my  (customer only) ─────────────────────────────
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyTickets([FromQuery] TicketFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1 || filter.PageSize > 50) filter.PageSize = 10;

        var userId = _userContext.GetCurrentAuthenticatedUserId();
        var pagination = await _serviceProviders.TicketService.HandleGetMyTickets(
            userId, filter.Status, filter.SortBy, filter.SortDirection, filter.Page, filter.PageSize);

        var response = ResponseMapper.MapToTicketListItemResponsePagination(pagination);
        return Ok(ApiResponse<Pagination<TicketListItemResponse>>.OkResponse(response));
    }

    // ── GET /api/tickets/my/{ticketId}  (customer only) ──────────────────
    [HttpGet("my/{ticketId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyTicketDetail([FromRoute] Guid ticketId)
    {
        var userId = _userContext.GetCurrentAuthenticatedUserId();
        var ticket = await _serviceProviders.TicketService.HandleGetMyTicketDetail(userId, ticketId);
        var response = ResponseMapper.MapToTicketResponse(ticket);
        return Ok(ApiResponse<TicketResponse>.OkResponse(response));
    }

    // ── POST /api/tickets/{ticketId}/messages  (customer + staff/admin) ──
    [HttpPost("{ticketId:guid}/messages")]
    [Authorize(Roles = "Customer, Staff, Admin")]
    public async Task<IActionResult> ReplyToTicket(
        [FromRoute] Guid ticketId,
        [FromBody] ReplyTicketRequest request)
    {
        var userId = _userContext.GetCurrentAuthenticatedUserId();
        var role = _userContext.GetCurrentUserRole();
        bool isStaff = role == Repository.Enums.UserRole.Staff || role == Repository.Enums.UserRole.Admin;

        var (message, ticket, notifyUserId) = await _serviceProviders.TicketService.HandleReplyToTicket(
            userId, ticketId, request.Content.Trim(), request.Attachments, isStaff);

        if (notifyUserId.HasValue)
        {
            var notificationPayload = new
            {
                TicketId = ticketId,
                Title = "Ticket có phản hồi mới",
                Message = $"Ticket \"{ticket.Title}\" vừa nhận được phản hồi mới."
            };
            await _notificationHub.Clients
                .Group($"user-{notifyUserId.Value}")
                .SendAsync(SignalRMessageConstant.TicketNotification, notificationPayload);
        }

        var response = ResponseMapper.MapToTicketMessageResponse(message);
        return Ok(ApiResponse<TicketMessageResponse>.OkResponse(response));
    }

    // ── GET /api/tickets/assigned  (staff/admin only) ────────────────────
    [HttpGet("assigned")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetAssignedTickets([FromQuery] TicketFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1 || filter.PageSize > 50) filter.PageSize = 10;

        var staffId = _userContext.GetCurrentAuthenticatedUserId();
        var pagination = await _serviceProviders.TicketService.HandleGetAssignedTickets(
            staffId, filter.Status, filter.SortBy, filter.SortDirection, filter.Page, filter.PageSize);

        var response = ResponseMapper.MapToTicketListItemResponsePagination(pagination);
        return Ok(ApiResponse<Pagination<TicketListItemResponse>>.OkResponse(response));
    }

    // ── GET /api/tickets/{ticketId}  (staff/admin only) ──────────────────
    [HttpGet("{ticketId:guid}")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetTicketDetail([FromRoute] Guid ticketId)
    {
        var ticket = await _serviceProviders.TicketService.HandleGetTicketDetail(ticketId);
        var response = ResponseMapper.MapToTicketResponse(ticket);
        return Ok(ApiResponse<TicketResponse>.OkResponse(response));
    }

    // ── PATCH /api/tickets/{ticketId}/status  (staff/admin only) ─────────
    [HttpPatch("{ticketId:guid}/status")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> UpdateTicketStatus(
        [FromRoute] Guid ticketId,
        [FromBody] UpdateTicketStatusRequest request)
    {
        var (ticket, notifyUserId) = await _serviceProviders.TicketService.HandleUpdateTicketStatus(
            ticketId, request.Status, request.Result);

        if (notifyUserId.HasValue)
        {
            var notificationPayload = new
            {
                TicketId = ticketId,
                Title = "Trạng thái ticket đã thay đổi",
                Message = $"Ticket \"{ticket.Title}\" đã chuyển sang trạng thái {request.Status}."
            };
            await _notificationHub.Clients
                .Group($"user-{notifyUserId.Value}")
                .SendAsync(SignalRMessageConstant.TicketNotification, notificationPayload);
        }

        var response = ResponseMapper.MapToTicketResponse(ticket);
        return Ok(ApiResponse<TicketResponse>.OkResponse(response));
    }

    // ── PATCH /api/tickets/{ticketId}/complete  (staff/admin only) ───────
    [HttpPatch("{ticketId:guid}/complete")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> CompleteTicket(
        [FromRoute] Guid ticketId,
        [FromBody] CompleteTicketRequest request)
    {
        var staffId = _userContext.GetCurrentAuthenticatedUserId();
        var (ticket, notifyUserId) = await _serviceProviders.TicketService.HandleCompleteTicket(
            staffId, ticketId, request.Status, request.Result.Trim());

        if (notifyUserId.HasValue)
        {
            var statusLabel = request.Status == TicketStatus.Resolved ? "đã được giải quyết" : "đã được đóng";
            var notificationPayload = new
            {
                TicketId = ticketId,
                Title = "Ticket của bạn đã được xử lý",
                Message = $"Ticket \"{ticket.Title}\" {statusLabel}. Kết quả: {request.Result.Trim()}"
            };
            await _notificationHub.Clients
                .Group($"user-{notifyUserId.Value}")
                .SendAsync(SignalRMessageConstant.TicketNotification, notificationPayload);
        }

        var response = ResponseMapper.MapToCompleteTicketResponse(ticket);
        return Ok(ApiResponse<CompleteTicketResponse>.OkResponse(response));
    }
}
