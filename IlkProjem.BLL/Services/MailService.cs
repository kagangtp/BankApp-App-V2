using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.MailDtos;
using IlkProjem.Core.Enums;
using IlkProjem.Core.Exceptions;

namespace IlkProjem.BLL.Services;

public class MailManager : IMailService
{
    private readonly IConfiguration _configuration;

    public MailManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendMailAsync(MailDto mailDto)
    {
        var email = new MimeMessage();
        
        // Gönderen Bilgisi (appsettings'ten çekilebilir)
        email.From.Add(MailboxAddress.Parse(_configuration["MailSettings:Mail"] ?? ""));
        email.To.Add(MailboxAddress.Parse(mailDto.To));
        email.Subject = mailDto.Subject ?? "No Subject";

        var builder = new BodyBuilder();
        builder.HtmlBody = mailDto.Body;
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        
        // SMTP Bağlantı Ayarları
        await smtp.ConnectAsync(
            _configuration["MailSettings:Host"], 
            int.Parse(_configuration["MailSettings:Port"] ?? ""), 
            SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(
            _configuration["MailSettings:Mail"], 
            _configuration["MailSettings:Password"]
        );

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public Task SendMailAsync1(MailDto mailDto)
    {
        throw new BusinessException(BusinessErrorCode.MailNotImplemented, "Bu metod henüz implemente edilmedi.");
    }
}