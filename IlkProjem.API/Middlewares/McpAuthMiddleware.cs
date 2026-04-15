// IlkProjem.API/Middlewares/McpAuthMiddleware.cs
public class McpAuthMiddleware(RequestDelegate next, IConfiguration config)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/mcp"))
        {
            var apiKey = config["MCP_API_KEY"];
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var key) || key != apiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
        await next(context);
    }
}