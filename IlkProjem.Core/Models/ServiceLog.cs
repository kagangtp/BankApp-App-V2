namespace IlkProjem.Core.Models;
public class ServiceLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string Path { get; set; }        // Örn: /api/Finance/Transfer
    public required string Method { get; set; }      // GET, POST...
    public required string RequestBody { get; set; } // Gelen veri
    public required string ResponseBody { get; set; } // Giden veri
    public int StatusCode { get; set; }     // 200, 500, 404...
    public string? Exception { get; set; }  // Hata varsa detayları
    public long DurationMs { get; set; }    // İşlem ne kadar sürdü?
    public string? InternalErrorCode { get; set; } // Örn: "BLL1001"
    public string? UserId { get; set; }             // İsteği yapan kullanıcı
}