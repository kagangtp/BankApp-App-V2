namespace IlkProjem.Core.Dtos.KnowledgeDtos;

public class KnowledgeSearchResultDto
{
    public string Content { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Score { get; set; }
}
