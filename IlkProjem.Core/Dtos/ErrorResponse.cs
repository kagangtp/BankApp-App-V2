namespace IlkProjem.Core.Dtos;

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageKey { get; set; }
    public string? ErrorCode { get; set; }
    public List<string>? Errors { get; set; }
}
