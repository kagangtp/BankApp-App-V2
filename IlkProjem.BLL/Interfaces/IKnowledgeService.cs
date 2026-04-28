using IlkProjem.Core.Dtos.KnowledgeDtos;
using Microsoft.AspNetCore.Http;

namespace IlkProjem.BLL.Interfaces;

/// <summary>
/// RAG bilgi tabanı yönetim servisi — doküman ekleme, arama, listeleme.
/// </summary>
public interface IKnowledgeService
{
    /// <summary>Dokümanı chunk'lar, embed eder ve veritabanına kaydeder.</summary>
    Task<string> IngestDocumentAsync(KnowledgeDocumentCreateDto dto, CancellationToken ct = default);

    /// <summary>PDF dosyasından metin çıkarır, chunk'lar, embed eder ve veritabanına kaydeder.</summary>
    Task<string> IngestPdfAsync(IFormFile file, string category = "General", string language = "tr", CancellationToken ct = default);

    /// <summary>Semantik benzerlik araması yapar, en yakın chunk'ları döner.</summary>
    Task<List<KnowledgeSearchResultDto>> SearchAsync(string query, int topK = 5, CancellationToken ct = default);

    /// <summary>Tüm dokümanları listeler.</summary>
    Task<List<KnowledgeDocumentReadDto>> ListDocumentsAsync(CancellationToken ct = default);

    /// <summary>Dokümanı ve chunk'larını siler.</summary>
    Task DeleteDocumentAsync(int documentId, CancellationToken ct = default);

    /// <summary>Dokümanı detaylı getirir.</summary>
    Task<KnowledgeDocumentDetailDto?> GetDocumentAsync(int id, CancellationToken ct = default);

    /// <summary>Dokümanı günceller (metni yeniden chunk'lar ve embed eder).</summary>
    Task<string> UpdateDocumentAsync(int id, KnowledgeDocumentUpdateDto dto, CancellationToken ct = default);
}
