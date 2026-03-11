using Microsoft.AspNetCore.Mvc;
using IlkProjem.BLL.Services;
using IlkProjem.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.Core.Exceptions;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly ICalculatorService _calculatorService;

    public CalculatorController(ICalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }

   [HttpGet("summation")]
    public IActionResult Sum(decimal a, decimal b) => Ok(_calculatorService.Sum(a, b));

    [HttpGet("difference")]
    public IActionResult Difference(decimal a, decimal b) => Ok(_calculatorService.Difference(a, b));
    [HttpGet("multiply")]
    public IActionResult Multiply(decimal a, decimal b) => Ok(_calculatorService.Multiply(a, b));

    [HttpGet("divide")]
    public IActionResult Divide(decimal a, decimal b)
    {
        try 
        {
            var result = _calculatorService.Divide(a, b);
            return Ok(result);
        }
        catch (DivideByZeroException ex)
        {
            // Kullanıcıya anlamlı bir hata mesajı dönüyoruz
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("system-stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        var count = await _calculatorService.GetTotalAccountCountAsync();
        var totalMoney = await _calculatorService.GetTotalBalanceSumAsync();

        return Ok(new 
        {
            TotalAccounts = count,
            GlobalBalance = totalMoney,
            AverageBalance = count > 0 ? (totalMoney / count) : 0,
            CalculationDate = DateTime.Now
        });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetTotalCount()
    {
        var count = await _calculatorService.GetTotalAccountCountAsync();
        return Ok(new { TotalAccounts = count });
    }

    [HttpGet("total-balance")]
    public async Task<IActionResult> GetTotalBalance()
    {
        var total = await _calculatorService.GetTotalBalanceSumAsync();
        return Ok(new { TotalBalance = total });
    }
    
}