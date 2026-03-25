using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.HouseDtos;
using IlkProjem.Core.Constants;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HouseController : ControllerBase
{
    private readonly IHouseService _houseService;

    public HouseController(IHouseService houseService)
    {
        _houseService = houseService;
    }

    [Authorize(Policy = Policies.CustomerManagement)]
    [HttpPost]
    public async Task<IActionResult> Create(HouseCreateDto createDto, CancellationToken ct)
    {
        var result = await _houseService.AddHouse(createDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId, CancellationToken ct)
    {
        var result = await _houseService.GetHousesByCustomerId(customerId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _houseService.GetHouseById(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [Authorize(Policy = Policies.CustomerManagement)]
    [HttpPut]
    public async Task<IActionResult> Update(HouseUpdateDto updateDto, CancellationToken ct)
    {
        var result = await _houseService.UpdateHouse(updateDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _houseService.DeleteHouse(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
