using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.WorkflowDtos;
using IlkProjem.Core.Enums;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Yeni workflow başlatır (DRAFT durumunda).
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start(WorkflowCreateDto createDto, CancellationToken ct)
    {
        var result = await _workflowService.StartWorkflow(createDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Draft durumundaki workflow'u günceller.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDraft(int id, WorkflowUpdateDto updateDto, CancellationToken ct)
    {
        updateDto.Id = id;
        var result = await _workflowService.UpdateDraft(updateDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// DRAFT → PENDING: Workflow'u onaya gönderir.
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        var result = await _workflowService.SubmitForApproval(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Müdür onay/red işlemi.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, WorkflowApproveDto approveDto, CancellationToken ct)
    {
        var result = await _workflowService.ProcessApproval(id, approveDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Workflow'u iptal eder.
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _workflowService.CancelWorkflow(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Workflow detayını getirir (Steps, Actions, History ile).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
    {
        var result = await _workflowService.GetWorkflowDetails(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Tüm workflow'ları listeler (opsiyonel status filtresi).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WorkflowStatus? status, CancellationToken ct)
    {
        var result = await _workflowService.GetWorkflows(status, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Giriş yapan müdürün bekleyen onaylarını getirir.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var result = await _workflowService.GetPendingApprovals(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Dashboard istatistiklerini getirir.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _workflowService.GetDashboardStats(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
