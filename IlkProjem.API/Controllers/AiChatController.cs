using IlkProjem.Core.Dtos.AiChatDtos;
using IlkProjem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;
    private readonly ICurrentUserService _currentUserService;

    public AiChatController(IAiChatService aiChatService, ICurrentUserService currentUserService)
    {
        _aiChatService = aiChatService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Kullanıcının mesajını AI asistana gönderir ve yanıtı döner.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] AiChatRequestDto request, CancellationToken ct)
    {
        var myId = _currentUserService.UserId;
        if (myId <= 0) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Mesaj boş olamaz." });

        var response = await _aiChatService.SendMessageAsync(myId, request.Message, ct);
        return Ok(response);
    }

    /// <summary>
    /// Kullanıcının AI ile olan sohbet geçmişini getirir.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var myId = _currentUserService.UserId;
        if (myId <= 0) return Unauthorized();

        var history = await _aiChatService.GetHistoryAsync(myId, ct);
        return Ok(history);
    }

    /// <summary>
    /// Kullanıcının AI sohbet geçmişini temizler.
    /// </summary>
    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory(CancellationToken ct)
    {
        var myId = _currentUserService.UserId;
        if (myId <= 0) return Unauthorized();

        await _aiChatService.ClearHistoryAsync(myId, ct);
        return NoContent();
    }
}
