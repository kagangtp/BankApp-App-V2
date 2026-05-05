using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Models;

public class WorkflowAction
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    
    /// <summary>
    /// Yapılan aksiyon: Approve, Reject, Comment, Cancel
    /// </summary>
    public WorkflowActionType Action { get; set; }
    
    /// <summary>
    /// Aksiyonu yapan kullanıcı.
    /// </summary>
    public int ByUserId { get; set; }
    public virtual User? ByUser { get; set; }
    
    /// <summary>
    /// Opsiyonel yorum.
    /// </summary>
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Workflow? Workflow { get; set; }
}
