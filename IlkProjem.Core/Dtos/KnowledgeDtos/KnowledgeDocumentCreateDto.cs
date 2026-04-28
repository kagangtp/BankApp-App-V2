namespace IlkProjem.Core.Dtos.KnowledgeDtos;

public class KnowledgeDocumentCreateDto
{
    /// <summary>Doküman başlığı</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Kategori: Policy, FAQ, Product, General</summary>
    public string Category { get; set; } = "General";

    /// <summary>Dil: "tr" veya "en"</summary>
    public string Language { get; set; } = "tr";

    /// <summary>Dokümanın tam içeriği — otomatik olarak chunk'lanıp embed edilir</summary>
    public string Content { get; set; } = string.Empty;
}
