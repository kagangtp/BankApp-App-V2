namespace IlkProjem.Core.Models;

/// <summary>
/// 1-on-1 chat mesajlarını veritabanında tutan entity.
/// BaseEntity'den türemez çünkü soft-delete / audit alanlarına ihtiyacı yok.
/// </summary>
public class ChatMessage
{
    public long Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
