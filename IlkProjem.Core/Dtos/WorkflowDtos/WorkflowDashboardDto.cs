namespace IlkProjem.Core.Dtos.WorkflowDtos;

/// <summary>
/// Dashboard istatistik DTO'su.
/// </summary>
public class WorkflowDashboardDto
{
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CompletedCount { get; set; }
    public int DraftCount { get; set; }
    public int TotalCount { get; set; }
}
