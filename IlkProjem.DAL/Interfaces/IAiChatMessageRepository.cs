using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;
public interface IAiChatMessageRepository : IGenericRepository<AiChatMessage>
{
    Task<List<AiChatMessage>> GetUserHistoryAsync(int userId, int limit = 50, CancellationToken ct = default);
    Task DeleteUserHistoryAsync(int userId, CancellationToken ct = default);
}
