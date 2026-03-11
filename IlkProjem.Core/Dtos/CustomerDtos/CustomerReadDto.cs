namespace IlkProjem.Core.Dtos.CustomerDtos;


public class CustomerReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ProfileImageId { get; set; }
    public string? ProfileImagePath { get; set; }
}