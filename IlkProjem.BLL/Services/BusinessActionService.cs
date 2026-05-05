using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.Core.Enums;
using IlkProjem.Core.Models;
using IlkProjem.Core.Utilities.Results;
using IlkProjem.DAL.Data;
using Microsoft.Extensions.Logging;

namespace IlkProjem.BLL.Services;

/// <summary>
/// Onay sonrası gerçek iş aksiyonlarını çalıştıran servis.
/// Workflow onaylandığında CustomerDelete/CustomerUpdate gibi işlemleri tetikler.
/// </summary>
public class BusinessActionService : IBusinessActionService
{
    private readonly ICustomerService _customerService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BusinessActionService> _logger;

    public BusinessActionService(
        ICustomerService customerService,
        AppDbContext dbContext,
        ILogger<BusinessActionService> logger)
    {
        _customerService = customerService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IResult> ExecuteAsync(Workflow workflow, CancellationToken ct = default)
    {
        _logger.LogInformation("BusinessAction çalıştırılıyor: {WorkflowNo} - {Type}", workflow.WorkflowNo, workflow.Type);

        try
        {
            switch (workflow.Type)
            {
                case WorkflowType.CustomerDelete:
                    return await ExecuteCustomerDelete(workflow, ct);

                case WorkflowType.CustomerUpdate:
                    return await ExecuteCustomerUpdate(workflow, ct);

                default:
                    return new ErrorResult($"Bilinmeyen workflow türü: {workflow.Type}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BusinessAction hatası: {WorkflowNo}", workflow.WorkflowNo);

            // AuditLog — hata kaydı
            _dbContext.Set<AuditLog>().Add(new AuditLog
            {
                WorkflowId = workflow.Id,
                EntityName = workflow.Type.ToString(),
                Action = "BusinessActionError",
                NewValue = ex.Message,
                ChangedByUserId = workflow.RequestedByUserId
            });
            await _dbContext.SaveChangesAsync(ct);

            return new ErrorResult($"İş aksiyonu hatası: {ex.Message}");
        }
    }

    private async Task<IResult> ExecuteCustomerDelete(Workflow workflow, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(workflow.Data))
            return new ErrorResult("Workflow verisi boş.");

        var data = JsonSerializer.Deserialize<JsonElement>(workflow.Data);
        if (!data.TryGetProperty("customerId", out var customerIdElement))
            return new ErrorResult("customerId bulunamadı.");

        var customerId = customerIdElement.GetInt32();

        // AuditLog — silme öncesi
        var customerResult = await _customerService.GetCustomerById(customerId, ct);
        var oldValue = customerResult.Success ? JsonSerializer.Serialize(customerResult.Data) : null;

        // Silme işlemi
        var deleteDto = new CustomerDeleteDto { Id = customerId };
        var result = await _customerService.DeleteCustomer(deleteDto, ct);

        // AuditLog
        _dbContext.Set<AuditLog>().Add(new AuditLog
        {
            WorkflowId = workflow.Id,
            EntityName = "Customer",
            Action = "Delete",
            OldValue = oldValue,
            ChangedByUserId = workflow.RequestedByUserId
        });
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Müşteri silindi: {CustomerId} via {WorkflowNo}", customerId, workflow.WorkflowNo);
        return result;
    }

    private async Task<IResult> ExecuteCustomerUpdate(Workflow workflow, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(workflow.Data))
            return new ErrorResult("Workflow verisi boş.");

        var updateDto = JsonSerializer.Deserialize<CustomerUpdateDto>(workflow.Data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (updateDto == null)
            return new ErrorResult("Workflow verisi parse edilemedi.");

        // AuditLog — güncelleme öncesi
        var customerResult = await _customerService.GetCustomerById(updateDto.Id, ct);
        var oldValue = customerResult.Success ? JsonSerializer.Serialize(customerResult.Data) : null;

        // Güncelleme işlemi
        var result = await _customerService.UpdateCustomer(updateDto, ct);

        // AuditLog
        _dbContext.Set<AuditLog>().Add(new AuditLog
        {
            WorkflowId = workflow.Id,
            EntityName = "Customer",
            Action = "Update",
            OldValue = oldValue,
            NewValue = workflow.Data,
            ChangedByUserId = workflow.RequestedByUserId
        });
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Müşteri güncellendi: {CustomerId} via {WorkflowNo}", updateDto.Id, workflow.WorkflowNo);
        return result;
    }
}
