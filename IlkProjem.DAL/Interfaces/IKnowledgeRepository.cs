using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;

public interface IKnowledgeRepository
{
    /// <summary>Vektör benzerlik araması — en yakın chunk'ları döner.</summary>
    Task<List<(KnowledgeChunk Chunk, double Distance)>> SearchSimilarAsync(float[] queryEmbedding, int topK = 5, CancellationToken ct = default);

    /// <summary>Doküman ve chunk'larını toplu kaydeder.</summary>
    Task AddDocumentWithChunksAsync(KnowledgeDocument doc, List<KnowledgeChunk> chunks, CancellationToken ct = default);

    /// <summary>Tüm dokümanları listeler (chunk'sız).</summary>
    Task<List<KnowledgeDocument>> GetAllDocumentsAsync(CancellationToken ct = default);

    /// <summary>Dokümanı ve cascade olarak chunk'larını siler.</summary>
    Task DeleteDocumentAsync(int documentId, CancellationToken ct = default);

    /// <summary>Bilgi tabanında herhangi bir doküman var mı?</summary>
    Task<bool> AnyAsync(CancellationToken ct = default);

    /// <summary>Dokümanı ID'sine göre getirir.</summary>
    Task<KnowledgeDocument?> GetDocumentByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Doküman değişikliklerini kaydeder.</summary>
    Task UpdateDocumentAsync(KnowledgeDocument doc, CancellationToken ct = default);
}
