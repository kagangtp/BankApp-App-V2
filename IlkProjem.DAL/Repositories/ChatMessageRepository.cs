using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(AppDbContext context) : base(context) { }

    public async Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2, int take = 50, CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .OrderBy(m => m.SentAt) 
            .ToListAsync(ct);
    }

    public async Task<List<ChatMessage>> GetRecentConversationsAsync(int userId, CancellationToken ct = default)
    {
        var latestDates = await _context.ChatMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.Max(m => m.SentAt))
            .ToListAsync(ct);

        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && latestDates.Contains(m.SentAt))
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(ct);

        return messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.First())
            .ToList();
    }

    public async Task MarkAsReadAsync(int senderId, int receiverId, CancellationToken ct = default)
    {
        await _context.ChatMessages
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
    }

    public async Task<bool> HasUnreadMessagesAsync(int userId, CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .AnyAsync(m => m.ReceiverId == userId && !m.IsRead, ct);
    }
}
