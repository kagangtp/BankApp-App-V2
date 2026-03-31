using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.UserDtos;
using IlkProjem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IlkProjem.Core.Constants;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (userId <= 0) return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId, ct);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [Authorize] // Sadece giriş yapmış olmak yeterli, böylece herkes chat yapabilir.
    [HttpGet]
    public async Task<IActionResult> GetAllUsersAsync(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    [Authorize(Policy = Policies.UserManagement)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserByIdAsync(int id, CancellationToken ct)
    {
        var user = await _userService.GetUserByIdAsync(id, ct);
        if (user == null) return NotFound($"User with ID {id} not found.");
        
        return Ok(user);
    }

    [Authorize(Policy = Policies.UserManagement)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserUpdateDto userUpdateDto, CancellationToken ct)
    {
        var user = await _userService.UpdateUserAsync(id, userUpdateDto, ct);
        return Ok(user);
    }

    [Authorize(Policy = Policies.UserManagement)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserAsync(int id, CancellationToken ct)
    {
        var deleted = await _userService.DeleteUserAsync(id, ct);
        if (!deleted) return NotFound($"User with ID {id} not found.");

        return NoContent();
    }

    [Authorize(Policy = Policies.UserManagement)]
    [HttpPut("{id}/promote")]
    public async Task<IActionResult> PromoteUserAsync(int id, CancellationToken ct)
    {
        var user = await _userService.PromoteUserAsync(id, ct);
        return Ok(user);
    }

    [Authorize(Policy = Policies.UserManagement)]
    [HttpPut("{id}/demote")]
    public async Task<IActionResult> DemoteUserAsync(int id, CancellationToken ct)
    {
        var user = await _userService.DemoteUserAsync(id, ct);
        return Ok(user);
    }
}
