using System.Text.Json.Serialization;

namespace IlkProjem.Core.Dtos.AuthDtos;

public class LoginResponseDto
{
    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}
