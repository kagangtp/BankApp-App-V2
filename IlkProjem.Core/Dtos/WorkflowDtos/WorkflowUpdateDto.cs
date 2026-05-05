namespace IlkProjem.Core.Dtos.WorkflowDtos;

/// <summary>
/// Draft durumundaki workflow güncellemesi için DTO.
/// </summary>
public class WorkflowUpdateDto
{
    public int Id { get; set; }
    
    /// <summary>
    /// Güncellenmiş işlem verileri (JSON).
    /// </summary>
    public string? Data { get; set; }
    
    /// <summary>
    /// Güncellenmiş açıklama.
    /// </summary>
    public string? Description { get; set; }
}
