using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class AiChatMessageRepository : GenericRepository<AiChatMessage>, IAiChatMessageRepository
{
    public AiChatMessageRepository(AppDbContext context) : base(context) { }

    public async Task<List<AiChatMessage>> GetUserHistoryAsync(int userId, int limit = 50, CancellationToken ct = default)
    {
        return await _context.AiChatMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .OrderBy(m => m.SentAt) 
            .ToListAsync(ct);
    }

    public async Task DeleteUserHistoryAsync(int userId, CancellationToken ct = default)
    {
        await _context.AiChatMessages
            .Where(m => m.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
