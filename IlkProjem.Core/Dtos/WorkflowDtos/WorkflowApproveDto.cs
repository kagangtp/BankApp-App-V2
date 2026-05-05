using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Dtos.WorkflowDtos;

/// <summary>
/// Müdür onay/red DTO'su.
/// </summary>
public class WorkflowApproveDto
{
    /// <summary>
    /// Onay mı red mi?
    /// </summary>
    public WorkflowActionType Action { get; set; }
    
    /// <summary>
    /// Opsiyonel yorum (özellikle red durumunda gerekçe).
    /// </summary>
    public string? Comment { get; set; }
}
