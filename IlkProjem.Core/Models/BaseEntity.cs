// IlkProjem.Core/Models/BaseEntity.cs
public abstract class BaseEntity
{
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    
    // Kayıt Bilgileri
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public int? CreatedByUserId { get; set; }

    // Güncelleme Bilgileri
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public int? UpdatedByUserId { get; set; }

    // Silme Bilgileri
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public int? DeletedByUserId { get; set; }
}