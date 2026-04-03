using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;

public interface IAiChatMessageRepository
{
    /// <summary>
    /// Kullanıcının AI ile olan mesaj geçmişini getirir (son N mesaj).
    /// </summary>
    Task<List<AiChatMessage>> GetUserHistoryAsync(int userId, int limit = 50, CancellationToken ct = default);

    Task AddAsync(AiChatMessage message, CancellationToken ct = default);

    /// <summary>
    /// Kullanıcının tüm AI sohbet geçmişini siler.
    /// </summary>
    Task DeleteUserHistoryAsync(int userId, CancellationToken ct = default);

    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}
