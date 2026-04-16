using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using IlkProjem.Core.Dtos.AiChatDtos;
using IlkProjem.Core.Interfaces;
using IlkProjem.Core.Models;
using IlkProjem.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IlkProjem.BLL.Services;

public class AiChatService : IAiChatService
{
    private readonly IAiChatMessageRepository _repo;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AiChatService> _logger;

    // Gemini API ayarları
    private readonly string _apiKey;
    private readonly string _model;

    public AiChatService(
        IAiChatMessageRepository repo,
        HttpClient httpClient,
        IConfiguration config,
        ILogger<AiChatService> logger)
    {
        _repo = repo;
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        _apiKey = _config["GeminiAI:ApiKey"] ?? "";
        _model = _config["GeminiAI:Model"] ?? "gemini-1.5-flash";
    }

    /// <summary>
    /// Banka asistanı sistem prompt'u.
    /// </summary>
    private const string SystemPrompt = @"You are BankApp AI Assistant — a professional banking assistant.
    - Always respond in the user's language (Turkish or English).
    - Answer only banking, finance, and BankApp-related questions.
    - Never invent interest rates, account data, or financial figures.
    - Be concise and use markdown formatting when helpful.";

    public async Task<AiChatMessageDto> SendMessageAsync(int userId, string message, CancellationToken ct = default)
    {
        // 1. Kullanıcı mesajını DB'ye kaydet
        var userMessage = new AiChatMessage
        {
            UserId = userId,
            Role = "user",
            Content = message,
            SentAt = DateTime.UtcNow
        };
        await _repo.AddAsync(userMessage, ct);
        await _repo.SaveChangesAsync(ct);

        // 2. Son 5 mesajı context olarak al
        var history = await _repo.GetUserHistoryAsync(userId, 5, ct);

        // 3. Gemini API'ye gönder
        string aiResponse;
        try
        {
            aiResponse = await CallGeminiApiAsync(history, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısı başarısız oldu. UserId: {UserId}", userId);
            aiResponse = "Üzgünüm, şu anda yanıt üretemiyorum. Lütfen daha sonra tekrar deneyin.";
        }

        // 4. AI yanıtını DB'ye kaydet
        var assistantMessage = new AiChatMessage
        {
            UserId = userId,
            Role = "assistant",
            Content = aiResponse,
            SentAt = DateTime.UtcNow
        };
        await _repo.AddAsync(assistantMessage, ct);
        await _repo.SaveChangesAsync(ct);

        // 5. DTO olarak dön
        return new AiChatMessageDto
        {
            Id = assistantMessage.Id,
            Role = assistantMessage.Role,
            Content = assistantMessage.Content,
            SentAt = assistantMessage.SentAt
        };
    }

    public async Task<List<AiChatMessageDto>> GetHistoryAsync(int userId, CancellationToken ct = default)
    {
        var messages = await _repo.GetUserHistoryAsync(userId, 50, ct);

        return messages.Select(m => new AiChatMessageDto
        {
            Id = m.Id,
            Role = m.Role,
            Content = m.Content,
            SentAt = m.SentAt
        }).ToList();
    }

    public async Task ClearHistoryAsync(int userId, CancellationToken ct = default)
    {
        await _repo.DeleteUserHistoryAsync(userId, ct);
    }

    /// <summary>
    /// Gemini API'ye HTTP isteği gönderir ve yanıtı döner.
    /// Her adımda null ve property kontrolü yaparak 500 hatasını engeller.
    /// </summary>
    private async Task<string> CallGeminiApiAsync(List<AiChatMessage> history, CancellationToken ct)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var contents = new List<object>();
        foreach (var msg in history)
        {
            contents.Add(new
            {
                role = msg.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = msg.Content ?? "" } }
            });
        }

        var requestBody = new
        {
            systemInstruction = new { parts = new[] { new { text = SystemPrompt } } },
            contents,
            generationConfig = new { temperature = 0.4, topP = 0.9, topK = 40, maxOutputTokens = 2048 }
        };

        const int maxRetries = 3;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
                
                // --- SAFE PARSING ---
                if (json?.RootElement.TryGetProperty("candidates", out var candidates) == true && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    
                    // Engel (Safety) kontrolü
                    if (firstCandidate.TryGetProperty("finishReason", out var reason) && reason.GetString() == "SAFETY")
                    {
                        return "Üzgünüm, güvenlik politikalarım gereği bu mesajı yanıtlayamıyorum.";
                    }

                    if (firstCandidate.TryGetProperty("content", out var content) && 
                        content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].TryGetProperty("text", out var textProp) ? textProp.GetString() : null;
                        return text ?? "Yanıt içeriği boş döndü.";
                    }
                }
                
                _logger.LogWarning("Gemini API yanıtı beklenen formatta değil veya engellenmiş olabilir.");
                return "Üzgünüm, şu an bu isteği işleyemiyorum.";
            }

            if ((int)response.StatusCode == 429 && attempt < maxRetries)
            {
                var waitSeconds = (int)Math.Pow(2, attempt + 1);
                _logger.LogWarning("Gemini API 429 Rate Limit. {WaitSeconds}s bekleniyor...", waitSeconds);
                await Task.Delay(waitSeconds * 1000, ct);
                continue;
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Gemini API Error: {StatusCode} — {Body}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
        }

        throw new HttpRequestException("Gemini API: Maximum retries reached.");
    }
}
