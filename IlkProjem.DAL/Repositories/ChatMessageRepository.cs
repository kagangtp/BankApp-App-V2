using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _context;

    public ChatMessageRepository(AppDbContext context) => _context = context;

    public async Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2, int take = 50, CancellationToken ct = default)
    {
        // İki kullanıcı arasındaki mesajları çek (her iki yönde de)
        // Son 'take' mesajı getir, sonra kronolojik sıraya çevir
        return await _context.ChatMessages
            .Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .OrderBy(m => m.SentAt) // UI'da doğru sırada göstermek için tekrar sırala
            .ToListAsync(ct);
    }

    public async Task<List<ChatMessage>> GetRecentConversationsAsync(int userId, CancellationToken ct = default)
    {
        // EF Core 7/8 GroupBy().First() hatasını çözmek için iki aşamalı sorgu
        
        // 1. Her partner için en son mesajın tarihini bul
        var latestDates = await _context.ChatMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.Max(m => m.SentAt))
            .ToListAsync(ct);

        // 2. Bu tarihlere sahip olan ilgili orijinal mesaj nesnelerini çek
        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && latestDates.Contains(m.SentAt))
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(ct);

        // 3. Olası çoklu eşleşmeleri önlemek için bellekte filtrele
        return messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.First())
            .ToList();
    }

    public async Task MarkAsReadAsync(int senderId, int receiverId, CancellationToken ct = default)
    {
        // senderId'nin receiverId'ye gönderdiği okunmamış mesajları okundu yap
        await _context.ChatMessages
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
    }

    public async Task<bool> HasUnreadMessagesAsync(int userId, CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .AnyAsync(m => m.ReceiverId == userId && !m.IsRead, ct);
    }

    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
        => await _context.ChatMessages.AddAsync(message, ct);

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct) > 0;
}
