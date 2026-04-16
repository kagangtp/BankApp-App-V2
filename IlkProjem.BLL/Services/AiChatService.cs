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
    private const string SystemPrompt = @"You are BankApp AI Assistant — a helpful, professional, and friendly banking assistant.

Your responsibilities:
- Answer questions about banking, finance, loans, investments, and personal finance.
- Help users understand general banking concepts and terminology.
- Provide general financial advice and explanations.
- Be concise but thorough in your explanations.

Strict Rules:
- Always respond in the same language the user writes in (Turkish or English).
- Be professional but approachable.
- If you don't know something or are unsure, say 'I'm not sure about that' — NEVER make up facts or numbers.
- Do NOT invent specific interest rates, account details, or financial figures unless you are certain.
- Do NOT pretend to have access to the user's account data or bank records.
- Never share sensitive information or make specific investment recommendations.
- Use markdown formatting (bold, lists) for better readability when appropriate.
- Keep responses focused, factual, and relevant to banking/finance topics.
- If someone asks about BankApp specifically, explain it is a customer management and banking platform.";

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

        // 2. Son 10 mesajı context olarak al
        var history = await _repo.GetUserHistoryAsync(userId, 10, ct);

        // 3. Gemini API'ye gönder
        string aiResponse;
        try
        {
            aiResponse = await CallGeminiApiAsync(history, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısı başarısız oldu. UserId: {UserId}", userId);
            aiResponse = "Üzgünüm, şu anda yanıt üretemiyorum. Lütfen daha sonra tekrar deneyin. / Sorry, I cannot generate a response right now. Please try again later.";
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
    /// 429 (Rate Limit) hatası alırsa exponential backoff ile yeniden dener.
    /// </summary>
    private async Task<string> CallGeminiApiAsync(List<AiChatMessage> history, CancellationToken ct)
    {
        var url = $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

        // Gemini API request body oluştur
        var contents = new List<object>();

        foreach (var msg in history)
        {
            contents.Add(new
            {
                role = msg.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = msg.Content } }
            });
        }

        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = SystemPrompt } }
            },
            contents,
            generationConfig = new
            {
                temperature = 0.4,
                topP = 0.9,
                topK = 40,
                maxOutputTokens = 2048
            }
        };

        // Retry mekanizması (429 Rate Limit için)
        const int maxRetries = 3;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
                var text = json?.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return text ?? "Yanıt alınamadı.";
            }

            // 429 Rate Limit — bekle ve tekrar dene
            if ((int)response.StatusCode == 429 && attempt < maxRetries)
            {
                var waitSeconds = (int)Math.Pow(2, attempt + 1); // 2s, 4s, 8s
                _logger.LogWarning("Gemini API 429 Rate Limit. {WaitSeconds}s beklenip tekrar denenecek (deneme {Attempt}/{MaxRetries})", 
                    waitSeconds, attempt + 1, maxRetries);
                await Task.Delay(waitSeconds * 1000, ct);
                continue;
            }

            // Diğer hatalar — direkt fırlat
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Gemini API Error Detail: {StatusCode} — {Body}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Gemini API error: {response.StatusCode}. Details: {errorBody}");
        }

        throw new HttpRequestException("Gemini API: Maximum retries reached.");
    }
}
