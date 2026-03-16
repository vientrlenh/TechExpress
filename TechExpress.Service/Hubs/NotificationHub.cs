using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TechExpress.Service.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}

