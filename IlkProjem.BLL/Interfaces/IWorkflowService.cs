using IlkProjem.Core.Dtos.WorkflowDtos;
using IlkProjem.Core.Enums;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.BLL.Interfaces;

public interface IWorkflowService
{
    /// <summary>
    /// Yeni workflow başlatır (DRAFT durumunda).
    /// </summary>
    Task<IDataResult<WorkflowReadDto>> StartWorkflow(WorkflowCreateDto createDto, CancellationToken ct = default);
    
    /// <summary>
    /// Draft durumundaki workflow'u günceller.
    /// </summary>
    Task<IResult> UpdateDraft(WorkflowUpdateDto updateDto, CancellationToken ct = default);
    
    /// <summary>
    /// DRAFT → PENDING: Onaya gönderir ve müdüre atar.
    /// </summary>
    Task<IResult> SubmitForApproval(int workflowId, CancellationToken ct = default);
    
    /// <summary>
    /// Müdür onay/red işlemi.
    /// </summary>
    Task<IResult> ProcessApproval(int workflowId, WorkflowApproveDto approveDto, CancellationToken ct = default);
    
    /// <summary>
    /// Workflow'u iptal eder.
    /// </summary>
    Task<IResult> CancelWorkflow(int workflowId, CancellationToken ct = default);
    
    /// <summary>
    /// Workflow detayını getirir (Steps, Actions, History ile).
    /// </summary>
    Task<IDataResult<WorkflowReadDto>> GetWorkflowDetails(int workflowId, CancellationToken ct = default);
    
    /// <summary>
    /// Tüm workflow'ları listeler (opsiyonel durum filtresi).
    /// </summary>
    Task<IDataResult<List<WorkflowReadDto>>> GetWorkflows(WorkflowStatus? statusFilter = null, CancellationToken ct = default);
    
    /// <summary>
    /// Müdürün bekleyen onaylarını getirir.
    /// </summary>
    Task<IDataResult<List<WorkflowReadDto>>> GetPendingApprovals(CancellationToken ct = default);
    
    /// <summary>
    /// Dashboard istatistiklerini getirir.
    /// </summary>
    Task<IDataResult<WorkflowDashboardDto>> GetDashboardStats(CancellationToken ct = default);
}
