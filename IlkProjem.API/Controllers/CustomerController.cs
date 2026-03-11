using Microsoft.AspNetCore.Mvc;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Models;
using IlkProjem.Core.Dtos.SpecificationDtos;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IExcelService _excelService; 

    public CustomerController(ICustomerService customerService, IExcelService excelService)
    {
        _customerService = customerService;
        _excelService = excelService;
    }

    // "Getir" - Tüm Liste
    // [HttpGet] 
    // public async Task<IActionResult> Get() 
    // {
    //     var result = await _customerService.GetAllCustomers();
    //     return result.Success ? Ok(result) : BadRequest(result);
    // }

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
    [HttpPost] 
    public async Task<IActionResult> Post(CustomerCreateDto createDto, CancellationToken ct)
    {
        var result = await _customerService.AddCustomer(createDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // "Güncelle"
    [HttpPut] 
    public async Task<IActionResult> Update(CustomerUpdateDto updateDto, CancellationToken ct)
    {
        var result = await _customerService.UpdateCustomer(updateDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // "Sil"
    [HttpDelete] 
    public async Task<IActionResult> Delete(CustomerDeleteDto deleteDto, CancellationToken ct)
    {
        var result = await _customerService.DeleteCustomer(deleteDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
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