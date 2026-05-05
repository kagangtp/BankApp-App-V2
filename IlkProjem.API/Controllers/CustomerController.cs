using Microsoft.AspNetCore.Mvc;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Models;
using IlkProjem.Core.Dtos.SpecificationDtos;
using IlkProjem.Core.Dtos.WorkflowDtos;
using IlkProjem.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.Core.Utilities.Results;
using IlkProjem.Core.Constants;
using System.Text.Json;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IExcelService _excelService;
    private readonly IWorkflowService _workflowService;

    public CustomerController(
        ICustomerService customerService,
        IExcelService excelService,
        IWorkflowService workflowService)
    {
        _customerService = customerService;
        _excelService = excelService;
        _workflowService = workflowService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerSpecParams custParams, CancellationToken ct)
    {
        var result = await _customerService.GetCustomersAsync(custParams, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // "GetirById"
    [HttpGet("{id}")] 
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await _customerService.GetCustomerById(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // "Ekle"
    [Authorize(Policy = Policies.CustomerManagement)]
    [HttpPost] 
    public async Task<IActionResult> Post(CustomerCreateDto createDto, CancellationToken ct)
    {
        var result = await _customerService.AddCustomer(createDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // "Güncelle" — Artık direkt güncellemiyor, workflow oluşturup müdür onayına gönderiyor.
    [Authorize(Policy = Policies.CustomerManagement)]
    [HttpPut] 
    public async Task<IActionResult> Update(CustomerUpdateDto updateDto, CancellationToken ct)
    {
        // 1. Workflow oluştur (DRAFT)
        var createResult = await _workflowService.StartWorkflow(new WorkflowCreateDto
        {
            Type = WorkflowType.CustomerUpdate,
            Data = JsonSerializer.Serialize(updateDto),
            Description = $"Müşteri güncelleme talebi: ID={updateDto.Id}, Ad={updateDto.Name}"
        }, ct);

        if (!createResult.Success)
            return BadRequest(createResult);

        // 2. Otomatik onaya gönder (DRAFT → PENDING)
        var submitResult = await _workflowService.SubmitForApproval(createResult.Data.Id, ct);

        return Ok(new SuccessResult(
            $"Güncelleme talebi oluşturuldu ve müdür onayına gönderildi. Workflow: {createResult.Data.WorkflowNo}"));
    }

    // "Sil" — Artık direkt silmiyor, workflow oluşturup müdür onayına gönderiyor.
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete] 
    public async Task<IActionResult> Delete(CustomerDeleteDto deleteDto, CancellationToken ct)
    {
        // 1. Workflow oluştur (DRAFT)
        var createResult = await _workflowService.StartWorkflow(new WorkflowCreateDto
        {
            Type = WorkflowType.CustomerDelete,
            Data = JsonSerializer.Serialize(new { customerId = deleteDto.Id }),
            Description = $"Müşteri silme talebi: ID={deleteDto.Id}"
        }, ct);

        if (!createResult.Success)
            return BadRequest(createResult);

        // 2. Otomatik onaya gönder (DRAFT → PENDING)
        var submitResult = await _workflowService.SubmitForApproval(createResult.Data.Id, ct);

        return Ok(new SuccessResult(
            $"Silme talebi oluşturuldu ve müdür onayına gönderildi. Workflow: {createResult.Data.WorkflowNo}"));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel([FromQuery] CustomerSpecParams custParams, CancellationToken ct)
    {
        // 1. Veriyi BLL'den çek
        var result = await _customerService.GetCustomersAsync(custParams, ct);

        if (!result.Success)
            return BadRequest(result);

        // 2. Generic ExcelService'i kullanarak byte array'i al
        var fileContent = _excelService.GenerateExcel(result.Data ?? [], "Müşteri Listesi");

        // 3. Dosya ismini ve MIME tipini belirterek fırlat
        string fileName = $"Musteriler_{DateTime.Now:yyyyMMdd}.xlsx";
        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(fileContent, contentType, fileName);
    }
}