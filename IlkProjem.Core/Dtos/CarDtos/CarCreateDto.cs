namespace IlkProjem.Core.Dtos.CarDtos;

public class CarCreateDto
{
    public required string Plate { get; set; }
    public string? Description { get; set; }
    public int CustomerId { get; set; }
}
