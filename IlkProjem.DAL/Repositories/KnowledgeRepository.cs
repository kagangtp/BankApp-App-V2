using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class KnowledgeRepository : IKnowledgeRepository
{
    private readonly AppDbContext _context;

    public KnowledgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<(KnowledgeChunk Chunk, double Distance)>> SearchSimilarAsync(
        float[] queryEmbedding, int topK = 5, CancellationToken ct = default)
    {
        // 1. Tüm chunk'ları belleğe çek (veya performans için dil/kategori filtresi eklenebilir)
        var allChunks = await _context.KnowledgeChunks
            .Where(c => c.Embedding != null)
            .ToListAsync(ct);

        // 2. C# tarafında Cosine Similarity (Dot Product) hesapla
        // Gemini embedding'leri zaten normalized (birim uzunlukta) döner, 
        // bu yüzden Dot Product doğrudan Cosine Similarity'e eşittir.
        var scoredResults = allChunks
            .Select(chunk =>
            {
                double dotProduct = 0;
                for (int i = 0; i < queryEmbedding.Length; i++)
                {
                    dotProduct += queryEmbedding[i] * chunk.Embedding![i];
                }
                // Cosine Distance = 1 - Similarity (Dot Product)
                return new { Chunk = chunk, Distance = 1.0 - dotProduct };
            })
            .OrderBy(x => x.Distance)
            .Take(topK)
            .ToList();

        // 3. Doküman bilgisini yükle
        var resultChunks = scoredResults.Select(r => r.Chunk).ToList();
        var docIds = resultChunks.Select(c => c.DocumentId).Distinct().ToList();
        var docs = await _context.KnowledgeDocuments
            .Where(d => docIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        foreach (var chunk in resultChunks)
        {
            if (docs.TryGetValue(chunk.DocumentId, out var doc))
                chunk.Document = doc;
        }

        return scoredResults.Select(r => (r.Chunk, r.Distance)).ToList();
    }

    public async Task AddDocumentWithChunksAsync(KnowledgeDocument doc, List<KnowledgeChunk> chunks, CancellationToken ct = default)
    {
        await _context.KnowledgeDocuments.AddAsync(doc, ct);
        await _context.SaveChangesAsync(ct);

        foreach (var chunk in chunks)
            chunk.DocumentId = doc.Id;

        await _context.KnowledgeChunks.AddRangeAsync(chunks, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<KnowledgeDocument>> GetAllDocumentsAsync(CancellationToken ct = default)
    {
        return await _context.KnowledgeDocuments
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken ct = default)
    {
        var doc = await _context.KnowledgeDocuments.FindAsync([documentId], ct);
        if (doc != null)
        {
            _context.KnowledgeDocuments.Remove(doc); // Cascade deletes chunks
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        return await _context.KnowledgeDocuments.AnyAsync(ct);
    }

    public async Task<KnowledgeDocument?> GetDocumentByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.KnowledgeDocuments
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task UpdateDocumentAsync(KnowledgeDocument doc, CancellationToken ct = default)
    {
        _context.KnowledgeDocuments.Update(doc);
        await _context.SaveChangesAsync(ct);
    }
}
