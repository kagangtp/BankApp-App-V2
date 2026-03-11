namespace IlkProjem.Core.Dtos.MailDtos;
public class MailDto {
    public required string To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}