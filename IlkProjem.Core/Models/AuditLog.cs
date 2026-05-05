namespace IlkProjem.Core.Models;

public class AuditLog
{
    public int Id { get; set; }
    
    /// <summary>
    /// İlişkili workflow (varsa).
    /// </summary>
    public int? WorkflowId { get; set; }
    
    /// <summary>
    /// Etkilenen entity adı: "Customer", "House", vs.
    /// </summary>
    public required string EntityName { get; set; }
    
    /// <summary>
    /// Yapılan işlem: "Update", "Delete", "Create"
    /// </summary>
    public required string Action { get; set; }
    
    /// <summary>
    /// Eski değer (JSON).
    /// </summary>
    public string? OldValue { get; set; }
    
    /// <summary>
    /// Yeni değer (JSON).
    /// </summary>
    public string? NewValue { get; set; }
    
    /// <summary>
    /// İşlemi yapan kullanıcı.
    /// </summary>
    public int? ChangedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Workflow? Workflow { get; set; }
}
