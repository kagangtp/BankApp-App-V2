using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;
public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2, int take = 50, CancellationToken ct = default);
    Task<List<ChatMessage>> GetRecentConversationsAsync(int userId, CancellationToken ct = default);
    Task MarkAsReadAsync(int senderId, int receiverId, CancellationToken ct = default);
    Task<bool> HasUnreadMessagesAsync(int userId, CancellationToken ct = default);
}
