using System.Text.Json.Serialization;

namespace IlkProjem.Core.Dtos.AuthDtos;

public class LoginDto
{
    [JsonPropertyName("email")] // Angular'dan gelen küçük 'email'i yakalar
    public string Email { get; set; }

    [JsonPropertyName("password")] // Angular'dan gelen küçük 'password'ü yakalar
    public string Password { get; set; }
}