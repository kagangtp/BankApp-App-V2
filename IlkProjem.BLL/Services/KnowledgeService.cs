using System.Text;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.KnowledgeDtos;
using IlkProjem.Core.Models;
using IlkProjem.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace IlkProjem.BLL.Services;

/// <summary>
/// RAG bilgi tabanı yönetim servisi.
/// Doküman alma → chunking → embedding → saklama + semantik arama.
/// </summary>
public class KnowledgeService : IKnowledgeService
{
    private readonly IKnowledgeRepository _repo;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<KnowledgeService> _logger;

    private const int MaxChunkLength = 500; // karakter cinsinden chunk boyutu
    private const long MaxPdfSizeBytes = 10 * 1024 * 1024; // 10 MB

    public KnowledgeService(
        IKnowledgeRepository repo,
        IEmbeddingService embeddingService,
        ILogger<KnowledgeService> logger)
    {
        _repo = repo;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<string> IngestDocumentAsync(KnowledgeDocumentCreateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Doküman alınıyor: {Title}", dto.Title);

        // 1. Chunk'la
        var textChunks = ChunkText(dto.Content);
        _logger.LogInformation("{Title} → {Count} chunk oluşturuldu.", dto.Title, textChunks.Count);

        // 2. Her chunk'u embed et
        var knowledgeChunks = new List<KnowledgeChunk>();
        for (int i = 0; i < textChunks.Count; i++)
        {
            try
            {
                var embedding = await _embeddingService.EmbedDocumentAsync(textChunks[i], ct);
                knowledgeChunks.Add(new KnowledgeChunk
                {
                    Content = textChunks[i],
                    Embedding = embedding,
                    ChunkIndex = i
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chunk {Index} embed edilemedi, atlanıyor.", i);
            }

            // Rate limit koruma — free tier için chunk aralarında kısa bekleme
            if (i < textChunks.Count - 1)
                await Task.Delay(200, ct);
        }

        if (knowledgeChunks.Count == 0)
            return "Hiçbir chunk embed edilemedi. Doküman kaydedilmedi.";

        // 3. Kaydet
        var doc = new KnowledgeDocument
        {
            Title = dto.Title,
            Category = dto.Category,
            Language = dto.Language,
            OriginalContent = dto.Content,
            ChunkCount = knowledgeChunks.Count
        };

        await _repo.AddDocumentWithChunksAsync(doc, knowledgeChunks, ct);
        _logger.LogInformation("Doküman kaydedildi: {Title} ({Count} chunk)", dto.Title, knowledgeChunks.Count);

        return $"Doküman '{dto.Title}' başarıyla kaydedildi ({knowledgeChunks.Count} chunk).";
    }

    public async Task<string> IngestPdfAsync(IFormFile file, string category = "General", string language = "tr", CancellationToken ct = default)
    {
        // Doğrulama
        if (file == null || file.Length == 0)
            throw new ArgumentException("PDF dosyası boş veya eksik.");

        if (file.Length > MaxPdfSizeBytes)
            throw new ArgumentException($"PDF dosyası çok büyük. Maksimum boyut: {MaxPdfSizeBytes / (1024 * 1024)} MB.");

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Sadece PDF dosyaları desteklenir.");

        _logger.LogInformation("PDF alınıyor: {FileName} ({Size} bytes)", file.FileName, file.Length);

        // PDF'den metin çıkar
        var textBuilder = new StringBuilder();
        using var stream = file.OpenReadStream();
        using var pdfDocument = PdfDocument.Open(stream);

        foreach (var page in pdfDocument.GetPages())
        {
            var pageText = page.Text;
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                textBuilder.AppendLine(pageText);
                textBuilder.AppendLine(); // Sayfalar arası boşluk
            }
        }

        var extractedText = textBuilder.ToString().Trim();
        if (string.IsNullOrWhiteSpace(extractedText))
            throw new InvalidOperationException("PDF'den metin çıkarılamadı. Dosya taranmış (image-based) bir PDF olabilir.");

        _logger.LogInformation("PDF'den {CharCount} karakter çıkarıldı ({PageCount} sayfa): {FileName}",
            extractedText.Length, pdfDocument.NumberOfPages, file.FileName);

        // Mevcut text ingestion pipeline'ına delege et
        var title = Path.GetFileNameWithoutExtension(file.FileName);
        var dto = new KnowledgeDocumentCreateDto
        {
            Title = title,
            Category = category,
            Language = language,
            Content = extractedText
        };

        return await IngestDocumentAsync(dto, ct);
    }

    public async Task<List<KnowledgeSearchResultDto>> SearchAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        // 1. Sorguyu embed et
        var queryEmbedding = await _embeddingService.EmbedQueryAsync(query, ct);

        // 2. Benzerlik araması
        var results = await _repo.SearchSimilarAsync(queryEmbedding, topK, ct);

        return results.Select(r => new KnowledgeSearchResultDto
        {
            Content = r.Chunk.Content,
            DocumentTitle = r.Chunk.Document?.Title ?? "",
            Category = r.Chunk.Document?.Category ?? "",
            Score = 1.0 - r.Distance // Cosine distance → similarity (0-1)
        }).ToList();
    }

    public async Task<List<KnowledgeDocumentReadDto>> ListDocumentsAsync(CancellationToken ct = default)
    {
        var docs = await _repo.GetAllDocumentsAsync(ct);
        return docs.Select(d => new KnowledgeDocumentReadDto
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.Category,
            Language = d.Language,
            ChunkCount = d.ChunkCount,
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken ct = default)
    {
        await _repo.DeleteDocumentAsync(documentId, ct);
        _logger.LogInformation("Doküman silindi: {DocumentId}", documentId);
    }

    public async Task<KnowledgeDocumentDetailDto?> GetDocumentAsync(int id, CancellationToken ct = default)
    {
        var doc = await _repo.GetDocumentByIdAsync(id, ct);
        if (doc == null) return null;

        return new KnowledgeDocumentDetailDto
        {
            Id = doc.Id,
            Title = doc.Title,
            Category = doc.Category,
            Language = doc.Language,
            OriginalContent = doc.OriginalContent,
            ChunkCount = doc.ChunkCount,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt
        };
    }

    public async Task<string> UpdateDocumentAsync(int id, KnowledgeDocumentUpdateDto dto, CancellationToken ct = default)
    {
        var doc = await _repo.GetDocumentByIdAsync(id, ct);
        if (doc == null)
            throw new KeyNotFoundException("Doküman bulunamadı.");

        _logger.LogInformation("Doküman güncelleniyor: {Id} - {Title}", id, dto.Title);

        // 1. Yeni chunk'ları oluştur
        var textChunks = ChunkText(dto.Content);
        _logger.LogInformation("{Title} (Update) → {Count} chunk oluşturuldu.", dto.Title, textChunks.Count);

        var newChunks = new List<KnowledgeChunk>();
        for (int i = 0; i < textChunks.Count; i++)
        {
            try
            {
                var embedding = await _embeddingService.EmbedDocumentAsync(textChunks[i], ct);
                newChunks.Add(new KnowledgeChunk
                {
                    Content = textChunks[i],
                    Embedding = embedding,
                    ChunkIndex = i
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chunk {Index} embed edilemedi, atlanıyor.", i);
            }

            if (i < textChunks.Count - 1)
                await Task.Delay(200, ct);
        }

        if (newChunks.Count == 0)
            return "Güncelleme başarısız: Hiçbir chunk embed edilemedi.";

        // 2. Eski chunk'ları sil ve doküman bilgilerini güncelle
        doc.Title = dto.Title;
        doc.Category = dto.Category;
        doc.Language = dto.Language;
        doc.OriginalContent = dto.Content;
        doc.UpdatedAt = DateTime.UtcNow;
        doc.ChunkCount = newChunks.Count;

        // EF Core track ettiği için Chunks koleksiyonunu temizleyip yenilerini ekleyebiliriz
        doc.Chunks.Clear();
        foreach (var chunk in newChunks)
        {
            doc.Chunks.Add(chunk);
        }

        await _repo.UpdateDocumentAsync(doc, ct);

        return $"Doküman '{dto.Title}' başarıyla güncellendi ({newChunks.Count} chunk).";
    }

    // ── Chunking Engine ──────────────────────────────────────────────────────

    /// <summary>
    /// Metni paragraf ve cümle bazında chunk'lara böler.
    /// Her chunk max ~500 karakter.
    /// </summary>
    private static List<string> ChunkText(string text)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        // Önce paragraflara böl
        var paragraphs = text.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries);

        foreach (var para in paragraphs)
        {
            var trimmed = para.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.Length <= MaxChunkLength)
            {
                chunks.Add(trimmed);
            }
            else
            {
                // Uzun paragrafları cümlelere böl
                var sentences = SplitIntoSentences(trimmed);
                var current = "";

                foreach (var sentence in sentences)
                {
                    if (current.Length + sentence.Length + 1 > MaxChunkLength && current.Length > 0)
                    {
                        chunks.Add(current.Trim());
                        current = "";
                    }
                    current += " " + sentence;
                }

                if (!string.IsNullOrWhiteSpace(current))
                    chunks.Add(current.Trim());
            }
        }

        return chunks;
    }

    private static List<string> SplitIntoSentences(string text)
    {
        var sentences = new List<string>();
        var delimiters = new[] { ". ", "! ", "? ", ".\n", "!\n", "?\n" };
        var remaining = text;

        while (remaining.Length > 0)
        {
            int earliest = -1;
            int delimLen = 0;

            foreach (var d in delimiters)
            {
                var idx = remaining.IndexOf(d, StringComparison.Ordinal);
                if (idx >= 0 && (earliest == -1 || idx < earliest))
                {
                    earliest = idx;
                    delimLen = d.Length;
                }
            }

            if (earliest == -1)
            {
                sentences.Add(remaining.Trim());
                break;
            }

            var sentence = remaining[..(earliest + delimLen)].Trim();
            if (sentence.Length > 0)
                sentences.Add(sentence);

            remaining = remaining[(earliest + delimLen)..];
        }

        return sentences;
    }
}
