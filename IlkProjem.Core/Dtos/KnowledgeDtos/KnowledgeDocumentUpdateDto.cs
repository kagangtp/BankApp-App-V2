namespace IlkProjem.Core.Dtos.KnowledgeDtos;

public class KnowledgeDocumentUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Language { get; set; } = "tr";
    public string Content { get; set; } = string.Empty;
}
