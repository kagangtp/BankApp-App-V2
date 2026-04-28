namespace IlkProjem.Core.Models;

/// <summary>
/// Bilgi tabanındaki bir dokümanı temsil eder.
/// RAG sistemi bu dokümanları chunk'lara böler ve vektör olarak saklar.
/// </summary>
public class KnowledgeDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Policy, FAQ, Product, General
    public string Language { get; set; } = "tr"; // "tr" veya "en"
    public string OriginalContent { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public List<KnowledgeChunk> Chunks { get; set; } = [];
}
