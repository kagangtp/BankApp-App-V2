namespace IlkProjem.Core.Dtos.KnowledgeDtos;

public class KnowledgeDocumentReadDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
