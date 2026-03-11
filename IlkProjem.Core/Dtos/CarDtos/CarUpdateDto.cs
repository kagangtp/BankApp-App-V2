namespace IlkProjem.Core.Dtos.CarDtos;

public class CarUpdateDto
{
    public int Id { get; set; }
    public required string Plate { get; set; }
    public string? Description { get; set; }
}
