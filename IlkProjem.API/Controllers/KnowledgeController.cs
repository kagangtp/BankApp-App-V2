using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Constants;
using IlkProjem.Core.Dtos.KnowledgeDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IlkProjem.API.Controllers;

/// <summary>
/// RAG Bilgi Tabanı yönetim endpoint'leri.
/// Doküman ekleme, listeleme, silme ve semantik arama.
/// </summary>
[Authorize(Policy = Policies.AdminOnly)]
[ApiController]
[Route("api/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;

    public KnowledgeController(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    /// <summary>
    /// Yeni doküman ekler — otomatik chunk'lar ve embed eder.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> IngestDocument([FromBody] KnowledgeDocumentCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { error = "Title ve Content zorunludur." });

        var result = await _knowledgeService.IngestDocumentAsync(dto, ct);
        return Ok(new { message = result });
    }

    /// <summary>
    /// PDF dosyası yükler — metni çıkarır, chunk'lar ve embed eder.
    /// Maksimum dosya boyutu: 10 MB.
    /// </summary>
    [HttpPost("upload-pdf")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> UploadPdf(
        IFormFile file,
        [FromForm] string category = "General",
        [FromForm] string language = "tr",
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "PDF dosyası gereklidir." });

        try
        {
            var result = await _knowledgeService.IngestPdfAsync(file, category, language, ct);
            return Ok(new { message = result, fileName = file.FileName, sizeBytes = file.Length });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tüm dokümanları listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListDocuments(CancellationToken ct)
    {
        var docs = await _knowledgeService.ListDocumentsAsync(ct);
        return Ok(docs);
    }

    /// <summary>
    /// Dokümanı ve chunk'larını siler.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id, CancellationToken ct)
    {
        await _knowledgeService.DeleteDocumentAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Doküman detayını (orijinal metni ile birlikte) getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id, CancellationToken ct)
    {
        var doc = await _knowledgeService.GetDocumentAsync(id, ct);
        if (doc == null) return NotFound(new { error = "Doküman bulunamadı." });
        return Ok(doc);
    }

    /// <summary>
    /// Dokümanı günceller, yeniden chunk'lar ve embed eder.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] KnowledgeDocumentUpdateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { error = "Title ve Content zorunludur." });

        try
        {
            var result = await _knowledgeService.UpdateDocumentAsync(id, dto, ct);
            return Ok(new { message = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Semantik arama — debug/test endpoint.
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] KnowledgeSearchRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest(new { error = "Query zorunludur." });

        var results = await _knowledgeService.SearchAsync(request.Query, request.TopK, ct);
        return Ok(results);
    }
}

public class KnowledgeSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
}
