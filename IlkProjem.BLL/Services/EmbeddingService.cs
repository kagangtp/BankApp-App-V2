using System.Net.Http.Json;
using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IlkProjem.BLL.Services;

/// <summary>
/// Gemini Embedding API (gemini-embedding-001) ile metin vektörleştirme servisi.
/// Free tier ile çalışır — aynı GeminiAI:ApiKey kullanılır.
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(HttpClient httpClient, IConfiguration config, ILogger<EmbeddingService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["GeminiAI:ApiKey"] ?? "";
        _model = config["GeminiAI:EmbeddingModel"] ?? "gemini-embedding-001";
        _logger = logger;
    }

    public async Task<float[]> EmbedQueryAsync(string text, CancellationToken ct = default)
        => await EmbedAsync(text, "RETRIEVAL_QUERY", ct);

    public async Task<float[]> EmbedDocumentAsync(string text, CancellationToken ct = default)
        => await EmbedAsync(text, "RETRIEVAL_DOCUMENT", ct);

    private async Task<float[]> EmbedAsync(string text, string taskType, CancellationToken ct)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:embedContent?key={_apiKey}";

        var body = new
        {
            taskType,
            content = new { parts = new[] { new { text } } }
        };

        const int maxRetries = 3;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(url, body, ct);

            if (response.IsSuccessStatusCode)
            {
                var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
                if (doc == null) throw new InvalidOperationException("Embedding API boş yanıt döndürdü.");

                var values = doc.RootElement
                    .GetProperty("embedding")
                    .GetProperty("values")
                    .EnumerateArray()
                    .Select(v => v.GetSingle())
                    .ToArray();

                return values;
            }

            if ((int)response.StatusCode == 429 && attempt < maxRetries)
            {
                var wait = 2 * (int)Math.Pow(2, attempt);
                _logger.LogWarning("Embedding API 429. {Wait}s bekleniyor (deneme {A})", wait, attempt + 1);
                await Task.Delay(wait * 1000, ct);
                continue;
            }

            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Embedding API Error: {Status} — {Body}", response.StatusCode, err);
            throw new HttpRequestException($"Embedding API: {response.StatusCode} — {err}");
        }

        throw new HttpRequestException("Embedding API: max retry aşıldı.");
    }
}
