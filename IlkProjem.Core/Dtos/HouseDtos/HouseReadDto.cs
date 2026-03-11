using IlkProjem.Core.Dtos.FileDtos;

namespace IlkProjem.Core.Dtos.HouseDtos;

public class HouseReadDto
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CustomerId { get; set; }
    public List<FileReadDto> Images { get; set; } = [];
}
