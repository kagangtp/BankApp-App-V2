namespace IlkProjem.Core.Dtos.UserDtos;

public class UserUpdateDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? Role { get; set; }
}
