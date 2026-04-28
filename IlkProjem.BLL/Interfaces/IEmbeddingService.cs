namespace IlkProjem.BLL.Interfaces;

/// <summary>
/// Gemini Embedding API ile metin vektörleştirme servisi.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>Kullanıcı sorgusunu embed eder (RETRIEVAL_QUERY).</summary>
    Task<float[]> EmbedQueryAsync(string text, CancellationToken ct = default);

    /// <summary>Doküman parçasını embed eder (RETRIEVAL_DOCUMENT).</summary>
    Task<float[]> EmbedDocumentAsync(string text, CancellationToken ct = default);
}
