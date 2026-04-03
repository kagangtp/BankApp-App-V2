namespace IlkProjem.Core.Dtos.AiChatDtos;

/// <summary>
/// AI chat mesajı için API response DTO'su.
/// </summary>
public class AiChatMessageDto
{
    public long Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
