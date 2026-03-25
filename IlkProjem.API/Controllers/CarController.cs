using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.CarDtos;
using IlkProjem.Core.Constants;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CarController : ControllerBase
{
    private readonly ICarService _carService;

    public CarController(ICarService carService)
    {
        _carService = carService;
    }

    [Authorize(Policy = Policies.CustomerManagement)]
    [HttpPost]
    public async Task<IActionResult> Create(CarCreateDto createDto, CancellationToken ct)
    {
        var result = await _carService.AddCar(createDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId, CancellationToken ct)
    {
        var result = await _carService.GetCarsByCustomerId(customerId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _carService.GetCarById(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [Authorize(Policy = Policies.CustomerManagement)]
    [HttpPut]
    public async Task<IActionResult> Update(CarUpdateDto updateDto, CancellationToken ct)
    {
        var result = await _carService.UpdateCar(updateDto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = Policies.AdminOnly)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _carService.DeleteCar(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
