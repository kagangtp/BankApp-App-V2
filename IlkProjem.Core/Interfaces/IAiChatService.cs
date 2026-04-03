using IlkProjem.Core.Dtos.AiChatDtos;

namespace IlkProjem.Core.Interfaces;

public interface IAiChatService
{
    /// <summary>
    /// Kullanıcının mesajını AI'a gönderir, yanıtı döner ve her ikisini de DB'ye kaydeder.
    /// </summary>
    Task<AiChatMessageDto> SendMessageAsync(int userId, string message, CancellationToken ct = default);

    /// <summary>
    /// Kullanıcının AI ile olan sohbet geçmişini getirir.
    /// </summary>
    Task<List<AiChatMessageDto>> GetHistoryAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Kullanıcının AI sohbet geçmişini temizler.
    /// </summary>
    Task ClearHistoryAsync(int userId, CancellationToken ct = default);
}
