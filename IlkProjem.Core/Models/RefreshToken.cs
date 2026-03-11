namespace IlkProjem.Core.Models;

public class RefreshToken
{
    public int Id { get; set; }
    
    // Token değeri (hash'lenmiş olarak saklanacak)
    public required string Token { get; set; }
    
    // Hangi kullanıcıya ait
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Token süreleri
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // İptal edilme zamanı (null ise hâlâ aktif)
    public DateTime? RevokedAt { get; set; }
    
    // Token rotation: Bu token yerine hangi yeni token geldi
    public string? ReplacedByToken { get; set; }
    
    // Hangi cihaz/tarayıcıdan geldi (opsiyonel)
    public string? DeviceInfo { get; set; }
    
    // Yardımcı property'ler
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
