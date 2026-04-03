using IlkProjem.Core.Interfaces;
using IlkProjem.Core.Dtos.ChatDtos;
using IlkProjem.Core.Models;
using IlkProjem.DAL.Interfaces;

namespace IlkProjem.BLL.Services;

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _chatRepo;

    public ChatService(IChatMessageRepository chatRepo)
    {
        _chatRepo = chatRepo;
    }

    public async Task<List<ChatMessageDto>> GetConversationAsync(int userId1, int userId2, CancellationToken ct = default)
    {
        var messages = await _chatRepo.GetConversationAsync(userId1, userId2, 50, ct);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId.ToString(),
            ReceiverId = m.ReceiverId.ToString(),
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead
        }).ToList();
    }

    public async Task<ChatMessageDto> SaveMessageAsync(int senderId, int receiverId, string content, CancellationToken ct = default)
    {
        var message = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        await _chatRepo.AddAsync(message, ct);
        await _chatRepo.SaveChangesAsync(ct);

        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId.ToString(),
            ReceiverId = message.ReceiverId.ToString(),
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };
    }

    public async Task<List<ChatMessageDto>> GetRecentConversationsAsync(int userId, CancellationToken ct = default)
    {
        var messages = await _chatRepo.GetRecentConversationsAsync(userId, ct);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId.ToString(),
            ReceiverId = m.ReceiverId.ToString(),
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead
        }).ToList();
    }

    public async Task MarkAsReadAsync(int senderId, int receiverId, CancellationToken ct = default)
    {
        await _chatRepo.MarkAsReadAsync(senderId, receiverId, ct);
    }

    public async Task<bool> HasUnreadMessagesAsync(int userId, CancellationToken ct = default)
    {
        return await _chatRepo.HasUnreadMessagesAsync(userId, ct);
    }
}
