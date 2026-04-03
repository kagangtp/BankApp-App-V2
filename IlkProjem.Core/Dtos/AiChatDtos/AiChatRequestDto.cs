namespace IlkProjem.Core.Dtos.AiChatDtos;

/// <summary>
/// Kullanıcının AI asistana gönderdiği mesaj için request DTO'su.
/// </summary>
public class AiChatRequestDto
{
    public string Message { get; set; } = string.Empty;
}
