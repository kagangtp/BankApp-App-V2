using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Dtos.WorkflowDtos;

/// <summary>
/// Workflow okuma DTO'su — liste ve detay görünümlerinde kullanılır.
/// </summary>
public class WorkflowReadDto
{
    public int Id { get; set; }
    public string WorkflowNo { get; set; } = string.Empty;
    public WorkflowType Type { get; set; }
    public WorkflowStatus Status { get; set; }
    public string? Data { get; set; }
    public string? Description { get; set; }
    
    // Talep Eden
    public int RequestedByUserId { get; set; }
    public string? RequestedByUserName { get; set; }
    
    // Atanan Müdür
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Detay sayfasında kullanılacak alt koleksiyonlar
    public List<WorkflowStepReadDto>? Steps { get; set; }
    public List<WorkflowActionReadDto>? Actions { get; set; }
    public List<WorkflowHistoryReadDto>? History { get; set; }
}

/// <summary>
/// Workflow adımı okuma DTO'su.
/// </summary>
public class WorkflowStepReadDto
{
    public int Id { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// Workflow aksiyon okuma DTO'su.
/// </summary>
public class WorkflowActionReadDto
{
    public int Id { get; set; }
    public WorkflowActionType Action { get; set; }
    public int ByUserId { get; set; }
    public string? ByUserName { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Workflow durum geçmişi okuma DTO'su.
/// </summary>
public class WorkflowHistoryReadDto
{
    public int Id { get; set; }
    public WorkflowStatus FromStatus { get; set; }
    public WorkflowStatus ToStatus { get; set; }
    public int ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public DateTime ChangedAt { get; set; }
}
