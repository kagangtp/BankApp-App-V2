using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Models;

public class WorkflowHistory
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    
    /// <summary>
    /// Önceki durum.
    /// </summary>
    public WorkflowStatus FromStatus { get; set; }
    
    /// <summary>
    /// Yeni durum.
    /// </summary>
    public WorkflowStatus ToStatus { get; set; }
    
    /// <summary>
    /// Değişikliği yapan kullanıcı.
    /// </summary>
    public int ChangedByUserId { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Workflow? Workflow { get; set; }
}
