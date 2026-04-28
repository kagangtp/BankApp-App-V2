namespace IlkProjem.Core.Models;

/// <summary>
/// Bir dokümanın vektör olarak saklanmış parçası.
/// Vektör benzerlik araması yapılarak RAG bağlamı oluşturulur.
/// </summary>
public class KnowledgeChunk
{
    public long Id { get; set; }
    public int DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; } // 768-dim
    public int ChunkIndex { get; set; }

    // Navigation
    public KnowledgeDocument Document { get; set; } = null!;
}
