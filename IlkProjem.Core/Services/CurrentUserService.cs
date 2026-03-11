// WebAPI katmanında implemente edilir
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IlkProjem.Core.Interfaces;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // JWT içindeki Name ve NameIdentifier claim'lerini okur
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    public int UserId => int.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}