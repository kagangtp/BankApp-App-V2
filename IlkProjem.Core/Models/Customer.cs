namespace IlkProjem.Core.Models;

public class Customer : BaseEntity
{
    public int Id { get; set; } 
    public required string Name { get; set; }
    public string? Email { get; set; }
    public decimal Balance { get; set; }
    public string? TcKimlikNo { get; set; } //T.C. Kimlik Numarası
    public DateTime? BirthDate { get; set; }

    // --- DOSYA BAĞLANTISI BURADA BAŞLIYOR ---
    
    // 1. Foreign Key: Dosyanın Guid'ini burada tutacağız
    public Guid? ProfileImageId { get; set; } 

    // 2. Navigation Property: Kod tarafında direkt "customer.ProfileImage" diyebilmek için
    public virtual Files? ProfileImage { get; set; }

    public virtual ICollection<House> Houses { get; set; } = new List<House>();
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}