namespace IlkProjem.Core.Dtos.CustomerDtos;


public class CustomerCreateDto
{
    // Id göndermiyoruz çünkü veritabanı otomatik oluşturacak
    public required string Name { get; set; }
    public string? Email { get; set; }
}