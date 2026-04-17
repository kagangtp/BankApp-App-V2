// IlkProjem.BLL/Services/AiChatService.cs
using System.Net.Http.Json;
using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.AiChatDtos;
using IlkProjem.Core.Dtos.CarDtos;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.Core.Dtos.HouseDtos;
using IlkProjem.Core.Dtos.SpecificationDtos;
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
    private readonly ICustomerService _customerService;
    private readonly ICarService _carService;
    private readonly IHouseService _houseService;

    private readonly string _apiKey;
    private readonly string _model;

    public AiChatService(
        IAiChatMessageRepository repo,
        HttpClient httpClient,
        IConfiguration config,
        ILogger<AiChatService> logger,
        ICustomerService customerService,
        ICarService carService,
        IHouseService houseService)
    {
        _repo = repo;
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _customerService = customerService;
        _carService = carService;
        _houseService = houseService;
        _apiKey = _config["GeminiAI:ApiKey"] ?? "";
        _model = _config["GeminiAI:Model"] ?? "gemini-1.5-flash";
    }

    private const string SystemPrompt = @"Sen BankApp AI'sın — bir bankacılık asistanısın.
Kullanıcının diline göre (Türkçe veya İngilizce) yanıt ver.
Müşteri, araç ve gayrimenkul verileri için sağlanan araçları kullan. Sahte veri üretme.
Verileri düzgün biçimlendir. Kısa ve profesyonel ol.";

    // ── Gemini'ye tanıtılan araçlar ─────────────────────────────────────────
    private static readonly object[] FunctionDeclarations =
    [
        new {
            name = "get_customers",
            description = "Müşteri listesini sayfalı olarak getirir. İsim/email araması desteklenir.",
            parameters = new {
                type = "object",
                properties = new {
                    pageSize  = new { type = "integer", description = "Sayfa boyutu (varsayılan 10)" },
                    lastId    = new { type = "integer", description = "Cursor sayfalama için son ID" },
                    search    = new { type = "string",  description = "İsim veya email arama filtresi" }
                }
            }
        },
        new {
            name = "get_customer_by_id",
            description = "ID ile tek müşterinin detaylarını getirir.",
            parameters = new {
                type = "object",
                properties = new {
                    id = new { type = "integer", description = "Müşteri ID'si" }
                },
                required = new[] { "id" }
            }
        },
        new {
            name = "add_customer",
            description = "Yeni müşteri oluşturur.",
            parameters = new {
                type = "object",
                properties = new {
                    name      = new { type = "string", description = "Ad soyad (zorunlu)" },
                    email     = new { type = "string", description = "Email adresi" },
                    birthDate = new { type = "string", description = "Doğum tarihi YYYY-MM-DD" }
                },
                required = new[] { "name" }
            }
        },
        new {
            name = "update_customer",
            description = "Mevcut müşteri bilgilerini günceller.",
            parameters = new {
                type = "object",
                properties = new {
                    id         = new { type = "integer", description = "Müşteri ID'si" },
                    name       = new { type = "string",  description = "Yeni ad soyad" },
                    email      = new { type = "string",  description = "Yeni email" },
                    balance    = new { type = "number",  description = "Yeni bakiye" },
                    tcKimlikNo = new { type = "string",  description = "TC Kimlik No" },
                    birthDate  = new { type = "string",  description = "Doğum tarihi YYYY-MM-DD" }
                },
                required = new[] { "id", "name" }
            }
        },
        new {
            name = "delete_customer",
            description = "Müşteriyi sistemden siler.",
            parameters = new {
                type = "object",
                properties = new {
                    id = new { type = "integer", description = "Silinecek müşteri ID'si" }
                },
                required = new[] { "id" }
            }
        },
        new {
            name = "get_cars_by_customer",
            description = "Bir müşterinin tüm araçlarını listeler.",
            parameters = new {
                type = "object",
                properties = new {
                    customerId = new { type = "integer", description = "Müşteri ID'si" }
                },
                required = new[] { "customerId" }
            }
        },
        new {
            name = "add_car",
            description = "Müşteriye yeni araç ekler.",
            parameters = new {
                type = "object",
                properties = new {
                    customerId  = new { type = "integer", description = "Müşteri ID'si" },
                    plate       = new { type = "string",  description = "Plaka (örn: 34 ABC 123)" },
                    description = new { type = "string",  description = "Araç açıklaması" }
                },
                required = new[] { "customerId", "plate" }
            }
        },
        new {
            name = "get_houses_by_customer",
            description = "Bir müşterinin tüm gayrimenkullerini listeler.",
            parameters = new {
                type = "object",
                properties = new {
                    customerId = new { type = "integer", description = "Müşteri ID'si" }
                },
                required = new[] { "customerId" }
            }
        },
        new {
            name = "add_house",
            description = "Müşteriye gayrimenkul ekler.",
            parameters = new {
                type = "object",
                properties = new {
                    customerId  = new { type = "integer", description = "Müşteri ID'si" },
                    address     = new { type = "string",  description = "Adres (zorunlu)" },
                    description = new { type = "string",  description = "Açıklama" }
                },
                required = new[] { "customerId", "address" }
            }
        }
    ];

    // ── Public API ───────────────────────────────────────────────────────────

    public async Task<AiChatMessageDto> SendMessageAsync(int userId, string message, CancellationToken ct = default)
    {
        var userMsg = new AiChatMessage { UserId = userId, Role = "user", Content = message, SentAt = DateTime.UtcNow };
        await _repo.AddAsync(userMsg, ct);
        await _repo.SaveChangesAsync(ct);

        var history = await _repo.GetUserHistoryAsync(userId, 6, ct);

        string aiResponse;
        try
        {
            aiResponse = await RunFunctionCallingLoopAsync(history, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API failure for UserId: {UserId}", userId);
            aiResponse = "Şu an yoğunluk nedeniyle yanıt veremiyorum. Lütfen az sonra tekrar deneyin.";
        }

        var assistantMsg = new AiChatMessage { UserId = userId, Role = "assistant", Content = aiResponse, SentAt = DateTime.UtcNow };
        await _repo.AddAsync(assistantMsg, ct);
        await _repo.SaveChangesAsync(ct);

        return new AiChatMessageDto
        {
            Id = assistantMsg.Id,
            Role = assistantMsg.Role,
            Content = assistantMsg.Content,
            SentAt = assistantMsg.SentAt
        };
    }

    public async Task<List<AiChatMessageDto>> GetHistoryAsync(int userId, CancellationToken ct = default)
    {
        var messages = await _repo.GetUserHistoryAsync(userId, 50, ct);
        return messages.Select(m => new AiChatMessageDto
        {
            Id = m.Id, Role = m.Role, Content = m.Content, SentAt = m.SentAt
        }).ToList();
    }

    public async Task ClearHistoryAsync(int userId, CancellationToken ct = default)
        => await _repo.DeleteUserHistoryAsync(userId, ct);

    // ── Function Calling Loop ────────────────────────────────────────────────

    private async Task<string> RunFunctionCallingLoopAsync(List<AiChatMessage> history, CancellationToken ct)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        // Konuşma geçmişini hazırla
        var contents = new List<object>(history.Select(m => (object)new
        {
            role  = m.Role == "assistant" ? "model" : "user",
            parts = new[] { new { text = m.Content ?? "" } }
        }));

        // Gemini, birden fazla fonksiyon çağırabilir — döngüyle takip ediyoruz
        const int maxTurns = 6;
        for (int turn = 0; turn < maxTurns; turn++)
        {
            var requestBody = new
            {
                systemInstruction = new { parts = new[] { new { text = SystemPrompt } } },
                contents,
                tools             = new[] { new { function_declarations = FunctionDeclarations } },
                generationConfig  = new { temperature = 0.3, maxOutputTokens = 2048 }
            };

            var geminiDoc = await PostToGeminiAsync(url, requestBody, ct);
            var root      = geminiDoc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                return "Gemini'den yanıt alınamadı.";

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var content))
                return "Gemini'den içerik alınamadı.";

            // Bu turın model yanıtını konuşmaya ekle
            contents.Add(content);

            var parts = content.GetProperty("parts");

            // Fonksiyon çağrısı var mı?
            bool calledFunction = false;
            foreach (var part in parts.EnumerateArray())
            {
                if (!part.TryGetProperty("functionCall", out var funcCall)) continue;

                calledFunction = true;
                var funcName = funcCall.GetProperty("name").GetString()!;
                var funcArgs = funcCall.TryGetProperty("args", out var argsEl) ? argsEl : default;

                _logger.LogInformation("Gemini → fonksiyon çağrısı: {Func}", funcName);

                var funcResult = await ExecuteFunctionAsync(funcName, funcArgs, ct);

                // Fonksiyon sonucunu konuşmaya ekle
                contents.Add(new
                {
                    role  = "user",
                    parts = new[]
                    {
                        new
                        {
                            functionResponse = new
                            {
                                name     = funcName,
                                response = new { result = funcResult }
                            }
                        }
                    }
                });

                break; // Bir seferinde bir fonksiyon
            }

            // Fonksiyon çağrısı yoksa metin yanıtı döndür
            if (!calledFunction)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                        return text.GetString() ?? "Yanıt üretilemedi.";
                }
                return "Yanıt üretilemedi.";
            }
        }

        return "İşlem tamamlandı.";
    }

    // ── Gemini HTTP çağrısı (retry'lı) ──────────────────────────────────────

    private async Task<JsonDocument> PostToGeminiAsync(string url, object body, CancellationToken ct)
    {
        const int maxRetries = 4;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(url, body, ct);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<JsonDocument>(ct)
                    ?? throw new InvalidOperationException("Gemini boş yanıt döndürdü.");

            if ((int)response.StatusCode == 429 && attempt < maxRetries)
            {
                var wait = 3 * (int)Math.Pow(2, attempt);
                _logger.LogWarning("Gemini 429. {Wait}s bekleniyor (deneme {A})", wait, attempt + 1);
                await Task.Delay(wait * 1000, ct);
                continue;
            }

            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Gemini Error: {Status} — {Body}", response.StatusCode, err);
            throw new HttpRequestException($"Gemini: {response.StatusCode}");
        }

        throw new HttpRequestException("Gemini: max retry aşıldı.");
    }

    // ── Fonksiyon yürütücü ───────────────────────────────────────────────────

    private async Task<string> ExecuteFunctionAsync(string funcName, JsonElement args, CancellationToken ct)
    {
        var opts = new JsonSerializerOptions { WriteIndented = false };

        // Güvenli arg okuyucular
        int    Int(string k, int def = 0)          => args.ValueKind != JsonValueKind.Undefined && args.TryGetProperty(k, out var v) ? v.GetInt32()     : def;
        string? Str(string k)                       => args.ValueKind != JsonValueKind.Undefined && args.TryGetProperty(k, out var v) ? v.GetString()    : null;
        decimal Dec(string k, decimal def = 0)      => args.ValueKind != JsonValueKind.Undefined && args.TryGetProperty(k, out var v) ? v.GetDecimal()   : def;
        DateTime? Date(string k)                    => Str(k) is { } s ? DateTime.Parse(s) : null;

        try
        {
            object result = funcName switch
            {
                "get_customers"        => await _customerService.GetCustomersAsync(new CustomerSpecParams
                                          {
                                              PageSize = Int("pageSize", 10),
                                              LastId   = Int("lastId", 0),
                                              Search   = Str("search")
                                          }),
                "get_customer_by_id"   => await _customerService.GetCustomerById(Int("id")),
                "add_customer"         => await _customerService.AddCustomer(new CustomerCreateDto
                                          {
                                              Name      = Str("name")!,
                                              Email     = Str("email"),
                                              BirthDate = Date("birthDate")
                                          }),
                "update_customer"      => await _customerService.UpdateCustomer(new CustomerUpdateDto
                                          {
                                              Id         = Int("id"),
                                              Name       = Str("name")!,
                                              Email      = Str("email"),
                                              Balance    = Dec("balance"),
                                              TcKimlikNo = Str("tcKimlikNo"),
                                              BirthDate  = Date("birthDate")
                                          }),
                "delete_customer"      => await _customerService.DeleteCustomer(new CustomerDeleteDto { Id = Int("id") }),
                "get_cars_by_customer" => await _carService.GetCarsByCustomerId(Int("customerId")),
                "add_car"              => await _carService.AddCar(new CarCreateDto
                                          {
                                              CustomerId  = Int("customerId"),
                                              Plate       = Str("plate")!,
                                              Description = Str("description")
                                          }),
                "get_houses_by_customer" => await _houseService.GetHousesByCustomerId(Int("customerId")),
                "add_house"            => await _houseService.AddHouse(new HouseCreateDto
                                          {
                                              CustomerId  = Int("customerId"),
                                              Address     = Str("address")!,
                                              Description = Str("description")
                                          }),
                _                      => (object)new { error = $"Bilinmeyen fonksiyon: {funcName}" }
            };

            return JsonSerializer.Serialize(result, opts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fonksiyon yürütme hatası: {Func}", funcName);
            return JsonSerializer.Serialize(new { error = ex.Message }, opts);
        }
    }
}
