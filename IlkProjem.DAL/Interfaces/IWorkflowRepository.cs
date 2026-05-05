using IlkProjem.Core.Enums;
using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;

public interface IWorkflowRepository : IGenericRepository<Workflow>
{
    /// <summary>
    /// Workflow'u Steps, Actions, History ile birlikte getirir.
    /// </summary>
    Task<Workflow?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    
    /// <summary>
    /// Müdürün bekleyen onaylarını getirir.
    /// </summary>
    Task<List<Workflow>> GetPendingByManagerAsync(int userId, CancellationToken ct = default);
    
    /// <summary>
    /// Workflow numarasına göre getirir.
    /// </summary>
    Task<Workflow?> GetByWorkflowNoAsync(string workflowNo, CancellationToken ct = default);
    
    /// <summary>
    /// Tüm workflow'ları (filtreleme destekli) getirir.
    /// </summary>
    Task<List<Workflow>> GetAllWithDetailsAsync(WorkflowStatus? statusFilter = null, CancellationToken ct = default);
    
    /// <summary>
    /// Dashboard istatistiklerini getirir (status bazlı sayılar).
    /// </summary>
    Task<Dictionary<WorkflowStatus, int>> GetStatusCountsAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Yeni workflow numarası üretir: WF-2024-XXXX
    /// </summary>
    Task<string> GenerateWorkflowNoAsync(CancellationToken ct = default);
}
