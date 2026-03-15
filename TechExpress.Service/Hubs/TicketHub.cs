using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Repository;

namespace TechExpress.Service.Hubs;

[Authorize]
public class TicketHub(UnitOfWork unitOfWork) : Hub
{
    private readonly UnitOfWork _unitOfWork = unitOfWork;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(Guid.Parse(userId));
            if (user is not null && !user.IsCustomerUser())
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "staff");
            }
        }

        await base.OnConnectedAsync();
    }
}
