using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;

public interface IChatMessageRepository
{
    /// <summary>
    /// İki kullanıcı arasındaki mesaj geçmişini SentAt sırasına göre getirir.
    /// </summary>
    Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2, int take = 50, CancellationToken ct = default);
    Task<List<ChatMessage>> GetRecentConversationsAsync(int userId, CancellationToken ct = default);
    Task MarkAsReadAsync(int senderId, int receiverId, CancellationToken ct = default);
    Task<bool> HasUnreadMessagesAsync(int userId, CancellationToken ct = default);

    Task AddAsync(ChatMessage message, CancellationToken ct = default);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}
