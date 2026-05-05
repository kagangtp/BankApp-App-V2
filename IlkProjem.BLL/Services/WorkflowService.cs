using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Constants;
using IlkProjem.Core.Dtos.WorkflowDtos;
using IlkProjem.Core.Enums;
using IlkProjem.Core.Hubs;
using IlkProjem.Core.Interfaces;
using IlkProjem.Core.Models;
using IlkProjem.Core.Utilities.Results;
using IlkProjem.DAL.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace IlkProjem.BLL.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBusinessActionService _businessActionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public WorkflowService(
        IWorkflowRepository workflowRepository,
        IUserRepository userRepository,
        IBusinessActionService businessActionService,
        ICurrentUserService currentUserService,
        IHubContext<NotificationHub> hubContext)
    {
        _workflowRepository = workflowRepository;
        _userRepository = userRepository;
        _businessActionService = businessActionService;
        _currentUserService = currentUserService;
        _hubContext = hubContext;
    }

    // ========================================
    // 1. START WORKFLOW (DRAFT)
    // ========================================
    public async Task<IDataResult<WorkflowReadDto>> StartWorkflow(WorkflowCreateDto createDto, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;

        var workflowNo = await _workflowRepository.GenerateWorkflowNoAsync(ct);

        var workflow = new Workflow
        {
            WorkflowNo = workflowNo,
            Type = createDto.Type,
            Status = WorkflowStatus.Draft,
            Data = createDto.Data,
            Description = createDto.Description,
            RequestedByUserId = userId
        };

        // Tek adım: Müdür Onayı
        workflow.Steps.Add(new WorkflowStep
        {
            StepOrder = 1,
            StepName = "Müdür Onayı",
            Status = WorkflowStatus.Pending
        });

        // İlk geçmiş kaydı
        workflow.History.Add(new WorkflowHistory
        {
            FromStatus = WorkflowStatus.Draft,
            ToStatus = WorkflowStatus.Draft,
            ChangedByUserId = userId
        });

        await _workflowRepository.AddAsync(workflow, ct);

        var dto = MapToReadDto(workflow);
        return new SuccessDataResult<WorkflowReadDto>(dto, $"Workflow {workflowNo} oluşturuldu.");
    }

    // ========================================
    // 2. UPDATE DRAFT
    // ========================================
    public async Task<IResult> UpdateDraft(WorkflowUpdateDto updateDto, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdWithDetailsAsync(updateDto.Id, ct);
        if (workflow == null)
            return new ErrorResult("Workflow bulunamadı.");

        if (workflow.Status != WorkflowStatus.Draft)
            return new ErrorResult("Sadece taslak (Draft) durumundaki workflow düzenlenebilir.");

        workflow.Data = updateDto.Data;
        workflow.Description = updateDto.Description;

        await _workflowRepository.UpdateAsync(workflow, ct);
        return new SuccessResult("Workflow güncellendi.");
    }

    // ========================================
    // 3. SUBMIT FOR APPROVAL (DRAFT → PENDING)
    // ========================================
    public async Task<IResult> SubmitForApproval(int workflowId, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdWithDetailsAsync(workflowId, ct);
        if (workflow == null)
            return new ErrorResult("Workflow bulunamadı.");

        if (workflow.Status != WorkflowStatus.Draft)
            return new ErrorResult("Sadece taslak durumundaki workflow onaya gönderilebilir.");

        var userId = _currentUserService.UserId;

        // Müdür ata — Manager rolündeki ilk kullanıcıyı bul
        var managers = await _userRepository.ListAllAsync(ct);
        var manager = managers.FirstOrDefault(u => u.Role == Roles.Manager || u.Role == Roles.Admin);
        
        if (manager == null)
            return new ErrorResult("Sistemde onay verecek müdür bulunamadı.");

        // Durum geçişi
        var oldStatus = workflow.Status;
        workflow.Status = WorkflowStatus.Pending;
        workflow.AssignedToUserId = manager.Id;

        // Step'i güncelle
        var step = workflow.Steps.FirstOrDefault(s => s.StepOrder == 1);
        if (step != null)
        {
            step.AssignedToUserId = manager.Id;
            step.Status = WorkflowStatus.ManagerApproval;
        }

        // Geçmiş kaydı
        workflow.History.Add(new WorkflowHistory
        {
            FromStatus = oldStatus,
            ToStatus = WorkflowStatus.Pending,
            ChangedByUserId = userId
        });

        await _workflowRepository.UpdateAsync(workflow, ct);

        // SignalR bildirim — müdüre
        var userName = _currentUserService.UserName ?? "Sistem";
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            User = userName,
            Action = "WorkflowPendingApproval",
            Message = $"{userName} tarafından {workflow.WorkflowNo} numaralı {workflow.Type} talebi onayınıza gönderildi.",
            WorkflowId = workflow.Id,
            WorkflowNo = workflow.WorkflowNo
        }, ct);

        return new SuccessResult($"Workflow {workflow.WorkflowNo} onaya gönderildi.");
    }

    // ========================================
    // 4. PROCESS APPROVAL (PENDING → APPROVED / REJECTED)
    // ========================================
    public async Task<IResult> ProcessApproval(int workflowId, WorkflowApproveDto approveDto, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdWithDetailsAsync(workflowId, ct);
        if (workflow == null)
            return new ErrorResult("Workflow bulunamadı.");

        if (workflow.Status != WorkflowStatus.Pending && workflow.Status != WorkflowStatus.ManagerApproval)
            return new ErrorResult("Bu workflow onay aşamasında değil.");

        var userId = _currentUserService.UserId;

        var oldStatus = workflow.Status;

        // Aksiyon kaydı
        workflow.Actions.Add(new WorkflowAction
        {
            Action = approveDto.Action,
            ByUserId = userId,
            Comment = approveDto.Comment
        });

        if (approveDto.Action == WorkflowActionType.Approve)
        {
            workflow.Status = WorkflowStatus.Approved;

            // Step güncelle
            var step = workflow.Steps.FirstOrDefault(s => s.StepOrder == 1);
            if (step != null)
            {
                step.Status = WorkflowStatus.Approved;
                step.ApprovedAt = DateTime.UtcNow;
            }

            // Geçmiş
            workflow.History.Add(new WorkflowHistory
            {
                FromStatus = oldStatus,
                ToStatus = WorkflowStatus.Approved,
                ChangedByUserId = userId
            });

            await _workflowRepository.UpdateAsync(workflow, ct);

            // Business Action çalıştır
            var actionResult = await _businessActionService.ExecuteAsync(workflow, ct);
            
            if (actionResult.Success)
            {
                // APPROVED → COMPLETED
                workflow.Status = WorkflowStatus.Completed;
                workflow.History.Add(new WorkflowHistory
                {
                    FromStatus = WorkflowStatus.Approved,
                    ToStatus = WorkflowStatus.Completed,
                    ChangedByUserId = userId
                });
                await _workflowRepository.UpdateAsync(workflow, ct);
            }

            // SignalR
            var userName = _currentUserService.UserName ?? "Sistem";
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                User = userName,
                Action = "WorkflowApproved",
                Message = $"{workflow.WorkflowNo} numaralı talep {userName} tarafından onaylandı.",
                WorkflowId = workflow.Id,
                WorkflowNo = workflow.WorkflowNo
            }, ct);

            return new SuccessResult($"Workflow {workflow.WorkflowNo} onaylandı ve işlem tamamlandı.");
        }
        else if (approveDto.Action == WorkflowActionType.Reject)
        {
            workflow.Status = WorkflowStatus.Rejected;

            // Step güncelle
            var step = workflow.Steps.FirstOrDefault(s => s.StepOrder == 1);
            if (step != null)
            {
                step.Status = WorkflowStatus.Rejected;
            }

            // Geçmiş
            workflow.History.Add(new WorkflowHistory
            {
                FromStatus = oldStatus,
                ToStatus = WorkflowStatus.Rejected,
                ChangedByUserId = userId
            });

            await _workflowRepository.UpdateAsync(workflow, ct);

            // SignalR
            var userName = _currentUserService.UserName ?? "Sistem";
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                User = userName,
                Action = "WorkflowRejected",
                Message = $"{workflow.WorkflowNo} numaralı talep {userName} tarafından reddedildi. Gerekçe: {approveDto.Comment ?? "-"}",
                WorkflowId = workflow.Id,
                WorkflowNo = workflow.WorkflowNo
            }, ct);

            return new SuccessResult($"Workflow {workflow.WorkflowNo} reddedildi.");
        }

        return new ErrorResult("Geçersiz aksiyon.");
    }

    // ========================================
    // 5. CANCEL WORKFLOW
    // ========================================
    public async Task<IResult> CancelWorkflow(int workflowId, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdWithDetailsAsync(workflowId, ct);
        if (workflow == null)
            return new ErrorResult("Workflow bulunamadı.");

        if (workflow.Status == WorkflowStatus.Completed || workflow.Status == WorkflowStatus.Cancelled)
            return new ErrorResult("Tamamlanmış veya iptal edilmiş workflow tekrar iptal edilemez.");

        var userId = _currentUserService.UserId;

        // Sadece talebi oluşturan kişi veya müdür/admin iptal edebilir
        if (workflow.RequestedByUserId != userId)
        {
            var users = await _userRepository.ListAllAsync(ct);
            var currentUser = users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null || (currentUser.Role != Roles.Admin && currentUser.Role != Roles.Manager))
                return new ErrorResult("Bu workflow'u sadece talebi oluşturan kişi veya müdür iptal edebilir.");
        }

        var oldStatus = workflow.Status;
        workflow.Status = WorkflowStatus.Cancelled;

        workflow.Actions.Add(new WorkflowAction
        {
            Action = WorkflowActionType.Cancel,
            ByUserId = userId,
            Comment = "Workflow iptal edildi."
        });

        workflow.History.Add(new WorkflowHistory
        {
            FromStatus = oldStatus,
            ToStatus = WorkflowStatus.Cancelled,
            ChangedByUserId = userId
        });

        await _workflowRepository.UpdateAsync(workflow, ct);
        return new SuccessResult($"Workflow {workflow.WorkflowNo} iptal edildi.");
    }

    // ========================================
    // 6. GET WORKFLOW DETAILS
    // ========================================
    public async Task<IDataResult<WorkflowReadDto>> GetWorkflowDetails(int workflowId, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdWithDetailsAsync(workflowId, ct);
        if (workflow == null)
            return new ErrorDataResult<WorkflowReadDto>("Workflow bulunamadı.");

        var dto = MapToReadDto(workflow, includeDetails: true);
        return new SuccessDataResult<WorkflowReadDto>(dto);
    }

    // ========================================
    // 7. GET WORKFLOWS (LIST)
    // ========================================
    public async Task<IDataResult<List<WorkflowReadDto>>> GetWorkflows(WorkflowStatus? statusFilter = null, CancellationToken ct = default)
    {
        var workflows = await _workflowRepository.GetAllWithDetailsAsync(statusFilter, ct);
        var dtos = workflows.Select(w => MapToReadDto(w)).ToList();
        return new SuccessDataResult<List<WorkflowReadDto>>(dtos);
    }

    // ========================================
    // 8. GET PENDING APPROVALS (FOR MANAGER)
    // ========================================
    public async Task<IDataResult<List<WorkflowReadDto>>> GetPendingApprovals(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;

        var workflows = await _workflowRepository.GetPendingByManagerAsync(userId, ct);
        var dtos = workflows.Select(w => MapToReadDto(w)).ToList();
        return new SuccessDataResult<List<WorkflowReadDto>>(dtos);
    }

    // ========================================
    // 9. DASHBOARD STATS
    // ========================================
    public async Task<IDataResult<WorkflowDashboardDto>> GetDashboardStats(CancellationToken ct = default)
    {
        var counts = await _workflowRepository.GetStatusCountsAsync(ct);

        var dto = new WorkflowDashboardDto
        {
            DraftCount = counts.GetValueOrDefault(WorkflowStatus.Draft),
            PendingCount = counts.GetValueOrDefault(WorkflowStatus.Pending) 
                         + counts.GetValueOrDefault(WorkflowStatus.ManagerApproval),
            ApprovedCount = counts.GetValueOrDefault(WorkflowStatus.Approved),
            RejectedCount = counts.GetValueOrDefault(WorkflowStatus.Rejected),
            CompletedCount = counts.GetValueOrDefault(WorkflowStatus.Completed),
            TotalCount = counts.Values.Sum()
        };

        return new SuccessDataResult<WorkflowDashboardDto>(dto);
    }

    // ========================================
    // MAPPING HELPER
    // ========================================
    private static WorkflowReadDto MapToReadDto(Workflow w, bool includeDetails = false)
    {
        var dto = new WorkflowReadDto
        {
            Id = w.Id,
            WorkflowNo = w.WorkflowNo,
            Type = w.Type,
            Status = w.Status,
            Data = w.Data,
            Description = w.Description,
            RequestedByUserId = w.RequestedByUserId,
            RequestedByUserName = w.RequestedByUser?.Username,
            AssignedToUserId = w.AssignedToUserId,
            AssignedToUserName = w.AssignedToUser?.Username,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt
        };

        if (includeDetails)
        {
            dto.Steps = w.Steps.OrderBy(s => s.StepOrder).Select(s => new WorkflowStepReadDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                StepName = s.StepName,
                Status = s.Status,
                AssignedToUserId = s.AssignedToUserId,
                AssignedToUserName = s.AssignedToUser?.Username,
                ApprovedAt = s.ApprovedAt
            }).ToList();

            dto.Actions = w.Actions.OrderByDescending(a => a.CreatedAt).Select(a => new WorkflowActionReadDto
            {
                Id = a.Id,
                Action = a.Action,
                ByUserId = a.ByUserId,
                ByUserName = a.ByUser?.Username,
                Comment = a.Comment,
                CreatedAt = a.CreatedAt
            }).ToList();

            dto.History = w.History.OrderBy(h => h.ChangedAt).Select(h => new WorkflowHistoryReadDto
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedByUserId = h.ChangedByUserId,
                ChangedAt = h.ChangedAt
            }).ToList();
        }

        return dto;
    }
}
