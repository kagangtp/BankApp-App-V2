using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace IlkProjem.Core.Hubs;

[Authorize]
public class NotificationHub : Hub
{

}