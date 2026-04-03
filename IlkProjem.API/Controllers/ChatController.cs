using IlkProjem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ICurrentUserService _currentUserService;

    public ChatController(IChatService chatService, ICurrentUserService currentUserService)
    {
        _chatService = chatService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Sidebar için: her konuşma partnerinden son mesajı döner.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentConversations(CancellationToken ct)
    {
        var myId = _currentUserService.UserId;
        if (myId <= 0) return Unauthorized();

        var recent = await _chatService.GetRecentConversationsAsync(myId, ct);
        return Ok(recent);
    }

    /// <summary>
    /// Oturum açmış kullanıcı ile belirtilen kullanıcı arasındaki mesaj geçmişini getirir.
    /// </summary>
    [HttpGet("conversation/{otherUserId:int}")]
    public async Task<IActionResult> GetConversation(int otherUserId, CancellationToken ct)
    {
        var myId = _currentUserService.UserId;
        if (myId <= 0) return Unauthorized();

        await _chatService.MarkAsReadAsync(otherUserId, myId, ct);

        var messages = await _chatService.GetConversationAsync(myId, otherUserId, ct);
        return Ok(messages);
    }

    /// <summary>
    /// Global olarak okunmamış herhangi bir mesaj var mı kontrol eder (Sidebar bildirimi için).
    /// </summary>
    [HttpGet("has-unread")]
    public async Task<IActionResult> HasUnreadMessages(CancellationToken ct)
    {
        var myId = _currentUserService.UserId;
        if (myId <= 0) return Unauthorized();

        var hasUnread = await _chatService.HasUnreadMessagesAsync(myId, ct);
        return Ok(new { hasUnread });
    }
}
