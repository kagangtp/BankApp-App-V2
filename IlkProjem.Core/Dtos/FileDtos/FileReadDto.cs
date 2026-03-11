namespace IlkProjem.Core.Dtos.FileDtos;

public class FileReadDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
}
