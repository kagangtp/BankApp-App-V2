using System.Net;
using System.Text.Json;
using FluentValidation;
using IlkProjem.Core.Dtos;
using IlkProjem.Core.Exceptions;

namespace IlkProjem.API.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogException(context, ex);
            await HandleExceptionAsync(context, ex);
        }
    }

    private void LogException(HttpContext context, Exception exception)
    {
        var requestPath = context.Request.Path;
        var method = context.Request.Method;

        switch (exception)
        {
            // Beklenen hatalar → Warning seviyesi
            case AppValidationException:
            case ValidationException:
            case BusinessException:
            case NotFoundException:
            case KeyNotFoundException:
                _logger.LogWarning(exception,
                    "[{Method}] {Path} → {ExceptionType}: {Message}",
                    method, requestPath, exception.GetType().Name, exception.Message);
                break;

            // Güvenlik hataları → Error seviyesi
            case UnauthorizedException:
            case UnauthorizedAccessException:
            case ForbiddenAccessException:
                _logger.LogError(exception,
                    "🔒 [{Method}] {Path} → {ExceptionType}: {Message}",
                    method, requestPath, exception.GetType().Name, exception.Message);
                break;

            // Beklenmeyen hatalar → Critical seviyesi (stack trace dahil)
            default:
                _logger.LogCritical(exception,
                    "🔥 [{Method}] {Path} → Beklenmeyen hata: {Message}",
                    method, requestPath, exception.Message);
                break;
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ErrorResponse();

        switch (exception)
        {
            // 400 — Validation hatası (custom)
            case AppValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = validationEx.Message;
                response.MessageKey = validationEx.MessageKey;
                if (validationEx.ValidationErrors != null)
                {
                    response.Errors = validationEx.ValidationErrors
                        .Select(e => $"{e.Key}: {e.Value}")
                        .ToList();
                }
                break;

            // 400 — Validation hatası (FluentValidation)
            case ValidationException fluentValidationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Doğrulama hatası oluştu.";
                response.MessageKey = "ValidationError";
                response.Errors = fluentValidationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                break;

            // 400 — İş kuralı hatası
            case BusinessException businessEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = businessEx.Message;
                response.ErrorCode = businessEx.ErrorCode.ToString();
                response.MessageKey = businessEx.MessageKey;
                break;

            // 401 — Yetkilendirme hatası
            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = unauthorizedEx.Message;
                response.MessageKey = unauthorizedEx.MessageKey;
                break;

            // 401 — Built-in UnauthorizedAccessException
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Bu işlem için yetkiniz bulunmamaktadır.";
                response.MessageKey = "Unauthorized";
                break;

            // 403 — Erişim engellendi
            case ForbiddenAccessException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = forbiddenEx.Message;
                response.MessageKey = forbiddenEx.MessageKey;
                break;

            // 404 — Kaynak bulunamadı
            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                response.MessageKey = notFoundEx.MessageKey;
                break;

            // 404 — Built-in KeyNotFoundException
            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "İstenen kaynak bulunamadı.";
                response.MessageKey = "NotFound";
                break;

            // 500 — Beklenmeyen sunucu hatası
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _env.IsDevelopment()
                    ? exception.Message
                    : "Beklenmeyen bir sunucu hatası oluştu.";
                response.MessageKey = "InternalServerError";
                break;
        }

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
