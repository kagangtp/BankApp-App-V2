namespace IlkProjem.Core.Dtos.KnowledgeDtos;

public class KnowledgeDocumentDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Language { get; set; } = "tr";
    public string OriginalContent { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
