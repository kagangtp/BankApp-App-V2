namespace IlkProjem.Core.Dtos.ChatDtos;

/// <summary>
/// Chat mesajı için API response DTO'su.
/// SenderId/ReceiverId string olarak döner çünkü frontend tarafında string olarak kullanılıyor.
/// </summary>
public class ChatMessageDto
{
    public long Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}
