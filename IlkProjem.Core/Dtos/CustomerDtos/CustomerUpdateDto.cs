namespace IlkProjem.Core.Dtos.CustomerDtos;

public class CustomerUpdateDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }

    // Solid NO IRL: A mistake to include Balance in the update DTO, but we will ignore this for now
    public decimal Balance { get; set; }
    public string? TcKimlikNo { get; set; }
}