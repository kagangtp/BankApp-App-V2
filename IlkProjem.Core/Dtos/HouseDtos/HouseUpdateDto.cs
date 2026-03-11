namespace IlkProjem.Core.Dtos.HouseDtos;

public class HouseUpdateDto
{
    public int Id { get; set; }
    public required string Address { get; set; }
    public string? Description { get; set; }
}
