using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using IlkProjem.Core.Exceptions;
using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;

namespace IlkProjem.API.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var watch = Stopwatch.StartNew();

        // 1. Request Body'yi Oku
        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
        context.Request.Body.Position = 0;

        // 2. Response'u yakalamak için Stream ayarla
        var originalBodyStream = context.Response.Body;
        using var responseBodyProxy = new MemoryStream();
        context.Response.Body = responseBodyProxy;

        string? innerErrorCode = null;
        string? exceptionMessage = null;

        try
        {
            await _next(context); // Controller'a git
        }
        catch (Exception ex)
        {
            exceptionMessage = ex.Message;

            if (ex is BusinessException bEx)
            {
                innerErrorCode = bEx.ErrorCode.ToString();
            }

            throw; // ExceptionHandlerMiddleware yakalasın
        }
        finally
        {
            watch.Stop();

            // 3. Response içeriğini güvenli bir şekilde oku
            string responseText = string.Empty;
            var contentType = context.Response.ContentType ?? "";

            // EĞER RESPONSE BİR DOSYA (EXCEL, IMAGE VB.) İSE STRING OLARAK OKUMA
            bool isBinaryResponse = contentType.Contains("application/vnd.openxmlformats-officedocument") || 
                                    contentType.Contains("application/octet-stream") ||
                                    contentType.Contains("image/");

            if (!isBinaryResponse && responseBodyProxy.Length > 0)
            {
                responseBodyProxy.Position = 0;
                responseText = await new StreamReader(responseBodyProxy, Encoding.UTF8).ReadToEndAsync();
            }
            else if (isBinaryResponse)
            {
                responseText = $"[Binary Content: {contentType}]";
            }

            // 4. Log kaydı oluştur
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var log = new ServiceLog
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                RequestBody = requestBody,
                ResponseBody = responseText,
                StatusCode = context.Response.StatusCode,
                InternalErrorCode = innerErrorCode,
                Exception = exceptionMessage,
                DurationMs = watch.ElapsedMilliseconds,
                UserId = userId,
                Timestamp = DateTime.UtcNow // Eğer BaseEntity'den gelmiyorsa elle ekledik
            };

            // 5. Logu kaydederken hata oluşursa asıl işlemi (Excel'i) bozma
            try 
            {
                dbContext.ServiceLog.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                // Loglama hatasını sadece konsola yaz, uygulama durmasın
                Console.WriteLine($"KRİTİK HATA: Log DB'ye yazılamadı: {dbEx.Message}");
            }

            // 6. Response'u orjinal akışa geri ver (Kullanıcı dosyayı indirebilsin)
            responseBodyProxy.Position = 0;
            await responseBodyProxy.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
}