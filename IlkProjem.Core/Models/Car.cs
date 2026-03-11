namespace IlkProjem.Core.Models;

public class Car : BaseEntity
{
    public int Id { get; set; }
    public required string Plate { get; set; } // Plaka
    public string? Description { get; set; } // Örn: "Deniz manzaralı dubleks"
    
    // İlişki: Hangi müşteriye ait?
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

}