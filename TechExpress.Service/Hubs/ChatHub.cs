using System;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Repository;
using TechExpress.Service.Constants;

namespace TechExpress.Service.Hubs;

public class ChatHub(UnitOfWork unitOfWork) : Hub
{
    private readonly UnitOfWork _unitOfWork = unitOfWork;

    public async Task JoinSession(Guid sessionId, string? phone)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(sessionId);
        if (session is null) return;
        if (userIdStr is not null)
        {
            var userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId);
            if (user is null) return;
            if (user.IsCustomerUser() && userId != session.UserId) return;
        }
        else if (phone is not null)
        {
            if (session.Phone != phone)
            {
                return;
            }
        }
        else
        {
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{sessionId}");
    }

    public async Task LeaveSession(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{sessionId}");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(Guid.Parse(userId));
            if (user is not null && !user.IsCustomerUser())
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "staff");
            }
        }
        await base.OnConnectedAsync();
    }

    public async Task CustomerTyping(Guid sessionId)
    {
        await Clients.Group("staff").SendAsync(SignalRMessageConstant.ShowTypingIndicator);
    }

    public async Task StaffTyping(Guid sessionId)
    {
        await Clients.Group($"chat-{sessionId}").SendAsync(SignalRMessageConstant.ShowTypingIndicator);
    }

    public async Task CustomerStopTyping(Guid sessionId)
    {
        await Clients.Group("staff").SendAsync(SignalRMessageConstant.HideTypingIndicator);
    }

    public async Task StaffStopTyping(Guid sessionId)
    {
        await Clients.Group($"chat-{sessionId}").SendAsync(SignalRMessageConstant.HideTypingIndicator);
    }
}
