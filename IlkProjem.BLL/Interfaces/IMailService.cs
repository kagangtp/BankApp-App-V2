using IlkProjem.Core.Dtos.MailDtos; // DTO'nun olduğu yer

namespace IlkProjem.BLL.Interfaces;

public interface IMailService
{
    Task SendMailAsync(MailDto mailDto);
}