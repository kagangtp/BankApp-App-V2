using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.Core.Interfaces;

namespace IlkProjem.Core.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly IChatService _chatService;

    public NotificationHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessageToUser(string receiverId, string message)
    {
        var senderId = Context.UserIdentifier; // JWT'den (NameIdentifier claim'i üzerinden) gelen kullanıcının ID'si

        if (string.IsNullOrEmpty(senderId)) return;

        // 1. Mesajı veritabanına kaydet
        var savedMessage = await _chatService.SaveMessageAsync(
            int.Parse(senderId),
            int.Parse(receiverId),
            message
        );

        // 2. Kaydedilen mesajın ID'sini de gönder (frontend'de duplicate kontrolü için)
        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, receiverId, message, savedMessage.SentAt, savedMessage.Id);

        // 3. Mesajı, gönderen kişinin eğer başka açık sekmesi varsa onlara da (senkronizasyon için) gönder
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, receiverId, message, savedMessage.SentAt, savedMessage.Id);
    }
}