namespace IlkProjem.Core.Dtos.HouseDtos;

public class HouseCreateDto
{
    public required string Address { get; set; }
    public string? Description { get; set; }
    public int CustomerId { get; set; }
}
