using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service;
using TechExpress.Service.Constants;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketController(
    ServiceProviders serviceProviders,
    UserContext userContext,
    IHubContext<TicketHub> ticketHub) : ControllerBase
{
    private readonly ServiceProviders _serviceProviders = serviceProviders;
    private readonly UserContext _userContext = userContext;
    private readonly IHubContext<TicketHub> _ticketHub = ticketHub;


    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromHeader(Name = "X-CustomPC-Guest-Session")] string? sessionId, [FromBody] CreateTicketRequest request)
    {
        var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
        Ticket ticket;
        if (userIdStr is not null)
        {
            var userId = Guid.Parse(userIdStr);
            ticket = await _serviceProviders.TicketService.HandleCreateTicketForAuthenticatedUser(
                userId,
                request.Title.Trim(),
                request.Description.Trim(),
                request.Message.Trim(),
                request.Type,
                request.CustomPCId,
                request.OrderId,
                request.OrderItemId,
                request.Attachments
            );
        } else
        {
            ticket = await _serviceProviders.TicketService.HandleCreateTicketForUnauthenticatedUser(
                sessionId,
                request.FullName,
                request.Phone,
                request.Title.Trim(),
                request.Description.Trim(),
                request.Message.Trim(),
                request.Type,
                request.CustomPCId, 
                request.OrderId, 
                request.OrderItemId,
                request.Attachments
            );
        }
        var response = ResponseMapper.MapToTicketResponse(ticket);
        await _ticketHub.Clients.Group("staff")
            .SendAsync(SignalRMessageConstant.TicketUpdated, response);
        return CreatedAtAction(nameof(CreateTicket), ApiResponse<TicketResponse>.CreatedResponse(response));
    }

    [HttpGet]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetAllTickets([FromQuery] TicketFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1 || filter.PageSize > 50) filter.PageSize = 10;

        var pagination = await _serviceProviders.TicketService.HandleGetAllTickets(
            filter.Status, filter.SortBy, filter.SortDirection, filter.Page, filter.PageSize);

        var response = ResponseMapper.MapToTicketListItemResponsePagination(pagination);
        return Ok(ApiResponse<Pagination<TicketListItemResponse>>.OkResponse(response));
    }

    // ── GET /api/ticket/my  (customer or guest by phone) ─────────────────
    [HttpGet("my")]
    public async Task<IActionResult> GetMyTickets([FromQuery] TicketFilterRequest filter, [FromQuery] string? phone)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1 || filter.PageSize > 50) filter.PageSize = 10;

        var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
        Pagination<Ticket> pagination;

        if (userIdStr is not null)
        {
            var userId = Guid.Parse(userIdStr);
            pagination = await _serviceProviders.TicketService.HandleGetTicketsForCustomer(
                userId, filter.Status, filter.SortBy, filter.SortDirection, filter.Page, filter.PageSize);
        }
        else
        {
            pagination = await _serviceProviders.TicketService.HandleGetTicketsForGuest(
                phone, filter.Status, filter.SortBy, filter.SortDirection, filter.Page, filter.PageSize);
        }

        var response = ResponseMapper.MapToTicketListItemResponsePagination(pagination);
        return Ok(ApiResponse<Pagination<TicketListItemResponse>>.OkResponse(response));
    }

    // ── GET /api/ticket/my/{ticketId}  (customer or guest by phone) ──────
    [HttpGet("my/{ticketId:guid}")]
    public async Task<IActionResult> GetMyTicketDetail([FromRoute] Guid ticketId, [FromQuery] string? phone)
    {
        var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();

        Ticket ticket;
        if (userIdStr is not null)
        {
            var userId = Guid.Parse(userIdStr);
            ticket = await _serviceProviders.TicketService.HandleGetMyTicketDetail(userId, ticketId);
        }
        else
        {
            ticket = await _serviceProviders.TicketService.HandleGetTicketDetailForGuest(phone, ticketId);
        }

        var response = ResponseMapper.MapToTicketResponse(ticket);
        return Ok(ApiResponse<TicketResponse>.OkResponse(response));
    }


    [HttpPost("{ticketId:guid}/messages")]
    [AllowAnonymous]
    public async Task<IActionResult> ReplyToTicket(
        [FromRoute] Guid ticketId,
        [FromBody] ReplyTicketRequest request)
    {
        var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
        TicketMessage message;
        Ticket ticket;

        if (userIdStr is not null)
        {
            var userId = Guid.Parse(userIdStr);
            bool isStaff = User.IsInRole(nameof(UserRole.Staff)) || User.IsInRole(nameof(UserRole.Admin));

            (message, ticket) = await _serviceProviders.TicketService.HandleReplyToTicketForAuthenticatedUser(
                userId, ticketId, request.Content.Trim(), request.Attachments, isStaff);

            var response = ResponseMapper.MapToTicketMessageResponse(message);

            if (isStaff)
            {
                // Notify the ticket owner (authenticated user) if applicable
                if (ticket.UserId.HasValue)
                {
                    await _ticketHub.Clients
                        .Group($"user-{ticket.UserId.Value}")
                        .SendAsync(SignalRMessageConstant.TicketMessageReceived, response);
                }
            }
            else
            {
                // Customer replied — notify staff
                await _ticketHub.Clients
                    .Group("staff")
                    .SendAsync(SignalRMessageConstant.TicketMessageReceived, response);
            }

            return Ok(ApiResponse<TicketMessageResponse>.OkResponse(response));
        }
        else
        {
            (message, _) = await _serviceProviders.TicketService.HandleReplyToTicketForUnauthenticatedUser(
                ticketId, request.Phone?.Trim(), request.Content.Trim(), request.Attachments);

            var response = ResponseMapper.MapToTicketMessageResponse(message);

            // Guest replied — notify staff
            await _ticketHub.Clients
                .Group("staff")
                .SendAsync(SignalRMessageConstant.TicketMessageReceived, response);

            return Ok(ApiResponse<TicketMessageResponse>.OkResponse(response));
        }
    }

    [HttpGet("{ticketId:guid}")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetTicketDetail([FromRoute] Guid ticketId)
    {
        var ticket = await _serviceProviders.TicketService.HandleGetTicketDetail(ticketId);
        var response = ResponseMapper.MapToTicketResponse(ticket);
        return Ok(ApiResponse<TicketResponse>.OkResponse(response));
    }


    [HttpPatch("{ticketId:guid}/status")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> UpdateTicketStatus(
        [FromRoute] Guid ticketId,
        [FromBody] UpdateTicketStatusRequest request)
    {
        var ticket = await _serviceProviders.TicketService.HandleUpdateTicketStatus(
            ticketId, request.Status);

        var response = ResponseMapper.MapToTicketResponse(ticket);
        if (response.UserId.HasValue)
        {
            await _ticketHub.Clients
                .Group($"user-{response.UserId.Value}")
                .SendAsync(SignalRMessageConstant.TicketUpdated, response);
        }

        return Ok(ApiResponse<TicketResponse>.OkResponse(response));
    }


    [HttpPatch("{ticketId:guid}/complete")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> CompleteTicket(
        [FromRoute] Guid ticketId,
        [FromBody] CompleteTicketRequest request)
    {
        var staffId = _userContext.GetCurrentAuthenticatedUserId();
        var ticket = await _serviceProviders.TicketService.HandleCompleteTicket(
            staffId, ticketId, request.Status);

        var ticketRealtimeResponse = ResponseMapper.MapToTicketResponse(ticket);

        if (ticketRealtimeResponse.UserId.HasValue)
        {
            await _ticketHub.Clients
                .Group($"user-{ticketRealtimeResponse.UserId.Value}")
                .SendAsync(SignalRMessageConstant.TicketUpdated, ticketRealtimeResponse);
        }

        var response = ResponseMapper.MapToCompleteTicketResponse(ticket);
        return Ok(ApiResponse<CompleteTicketResponse>.OkResponse(response));
    }
}
