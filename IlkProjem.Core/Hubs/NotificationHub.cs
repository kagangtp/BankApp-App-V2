using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace IlkProjem.Core.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task SendMessageToUser(string receiverId, string message)
    {
        var senderId = Context.UserIdentifier; // JWT'den (NameIdentifier claim'i üzerinden) gelen kullanıcının ID'si

        if (string.IsNullOrEmpty(senderId)) return;

        // 1. Mesajı alıcıya gönder
        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, receiverId, message, DateTime.UtcNow);

        // 2. Mesajı, gönderen kişinin eğer başka açık sekmesi varsa onlara da (senkronizasyon için) gönder
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, receiverId, message, DateTime.UtcNow);
    }
}