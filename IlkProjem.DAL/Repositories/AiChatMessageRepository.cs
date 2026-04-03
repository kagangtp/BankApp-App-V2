using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class AiChatMessageRepository : IAiChatMessageRepository
{
    private readonly AppDbContext _context;

    public AiChatMessageRepository(AppDbContext context) => _context = context;

    public async Task<List<AiChatMessage>> GetUserHistoryAsync(int userId, int limit = 50, CancellationToken ct = default)
    {
        return await _context.AiChatMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .OrderBy(m => m.SentAt) // Kronolojik sıraya çevir
            .ToListAsync(ct);
    }

    public async Task AddAsync(AiChatMessage message, CancellationToken ct = default)
        => await _context.AiChatMessages.AddAsync(message, ct);

    public async Task DeleteUserHistoryAsync(int userId, CancellationToken ct = default)
    {
        await _context.AiChatMessages
            .Where(m => m.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct) > 0;
}
