using IlkProjem.Core.Dtos.FileDtos;

namespace IlkProjem.Core.Dtos.CarDtos;

public class CarReadDto
{
    public int Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CustomerId { get; set; }
    public List<FileReadDto> Images { get; set; } = [];
}
