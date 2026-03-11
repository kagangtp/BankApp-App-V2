using Microsoft.AspNetCore.Mvc;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.MailDtos;
using Microsoft.AspNetCore.Authorization;

namespace IlkProjem.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMail([FromBody] MailDto mailDto)
    {
        await _mailService.SendMailAsync(mailDto);
        return Ok(new { message = "Mail başarıyla gönderildi." });
    }
}