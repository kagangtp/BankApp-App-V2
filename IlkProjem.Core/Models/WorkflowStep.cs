using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Models;

public class WorkflowStep
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    
    /// <summary>
    /// Adım sırası (1, 2, 3...)
    /// </summary>
    public int StepOrder { get; set; }
    
    /// <summary>
    /// Adım adı: "Müdür Onayı"
    /// </summary>
    public required string StepName { get; set; }
    
    /// <summary>
    /// Bu adımın mevcut durumu.
    /// </summary>
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
    
    /// <summary>
    /// Bu adımda onay verecek kullanıcı.
    /// </summary>
    public int? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation
    public virtual Workflow? Workflow { get; set; }
}
