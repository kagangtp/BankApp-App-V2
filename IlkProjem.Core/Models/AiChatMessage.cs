namespace IlkProjem.Core.Models;

/// <summary>
/// Kullanıcı ile AI asistan arasındaki sohbet mesajlarını veritabanında tutan entity.
/// Role: "user" veya "assistant" olabilir.
/// </summary>
public class AiChatMessage
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = "user"; // "user" veya "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
