using System;
using IlkProjem.Core.Attributes;

namespace IlkProjem.Core.Dtos.CustomerDtos;

public class CustomerReadDto
{
    [ExcelIgnore]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    
    [ExcelIgnore]
    public string? TcKimlikNo { get; set; }
    [ExcelIgnore]
    public Guid? ProfileImageId { get; set; }
    
    [ExcelIgnore]
    public string? ProfileImagePath { get; set; }
}