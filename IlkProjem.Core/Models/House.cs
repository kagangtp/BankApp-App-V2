namespace IlkProjem.Core.Models;

public class House : BaseEntity
{
    public int Id { get; set; }
    public required string Address { get; set; }
    public string? Description { get; set; } // Örn: "Deniz manzaralı dubleks"
    
    // İlişki: Hangi müşteriye ait?
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

}