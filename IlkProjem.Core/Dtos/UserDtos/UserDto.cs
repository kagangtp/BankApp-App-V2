namespace IlkProjem.Core.Dtos.UserDtos;

public class UserDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
