using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Models;

public class Workflow : BaseEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// Benzersiz workflow numarası: WF-2024-0001
    /// </summary>
    public required string WorkflowNo { get; set; }
    
    /// <summary>
    /// İşlem türü: CustomerDelete, CustomerUpdate, vs.
    /// </summary>
    public WorkflowType Type { get; set; }
    
    /// <summary>
    /// Durum makinesi mevcut durumu.
    /// </summary>
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    
    /// <summary>
    /// İşlem verileri JSON olarak (müşteri id, eski değer, yeni değer, vs.)
    /// </summary>
    public string? Data { get; set; }
    
    /// <summary>
    /// Talep açıklaması — kullanıcı tarafından yazılır.
    /// </summary>
    public string? Description { get; set; }
    
    // --- Talep Eden Kullanıcı ---
    public int RequestedByUserId { get; set; }
    public virtual User? RequestedByUser { get; set; }
    
    // --- Atanan Müdür (Onaylayıcı) ---
    public int? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }
    
    // --- Navigation Properties ---
    public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public virtual ICollection<WorkflowAction> Actions { get; set; } = new List<WorkflowAction>();
    public virtual ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
}
