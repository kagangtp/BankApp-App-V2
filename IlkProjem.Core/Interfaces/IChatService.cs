using IlkProjem.Core.Dtos.ChatDtos;

namespace IlkProjem.Core.Interfaces;

public interface IChatService
{
    /// <summary>
    /// İki kullanıcı arasındaki mesaj geçmişini getirir (son N mesaj).
    /// </summary>
    Task<List<ChatMessageDto>> GetConversationAsync(int userId1, int userId2, CancellationToken ct = default);
    Task<ChatMessageDto> SaveMessageAsync(int senderId, int receiverId, string content, CancellationToken ct = default);
    Task<List<ChatMessageDto>> GetRecentConversationsAsync(int userId, CancellationToken ct = default);
    Task MarkAsReadAsync(int senderId, int receiverId, CancellationToken ct = default);
    Task<bool> HasUnreadMessagesAsync(int userId, CancellationToken ct = default);
}
