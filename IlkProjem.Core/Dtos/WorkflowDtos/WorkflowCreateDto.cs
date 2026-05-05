using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Dtos.WorkflowDtos;

/// <summary>
/// Yeni workflow başlatma DTO'su.
/// </summary>
public class WorkflowCreateDto
{
    /// <summary>
    /// İşlem türü: CustomerDelete, CustomerUpdate
    /// </summary>
    public WorkflowType Type { get; set; }
    
    /// <summary>
    /// İşlem verileri JSON olarak.
    /// CustomerDelete: {"customerId": 123}
    /// CustomerUpdate: {"customerId": 123, "name": "Yeni İsim", "email": "yeni@mail.com", ...}
    /// </summary>
    public string? Data { get; set; }
    
    /// <summary>
    /// Talep açıklaması.
    /// </summary>
    public string? Description { get; set; }
}
