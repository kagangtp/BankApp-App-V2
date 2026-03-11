using Microsoft.AspNetCore.Mvc;

using IlkProjem.BLL.Interfaces;
using IlkProjem.BLL.Services;
using IlkProjem.Core.Dtos.AuthDtos;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            await _authService.Register(registerDto);
            return Ok(new { message = "Kayıt işlemi başarıyla tamamlandı!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken ct)
    {
        var result = await _authService.Login(loginDto, ct);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        // LoginDataResult'tan raw refresh token'ı al ve cookie'ye yaz
        if (result is LoginDataResult loginResult)
        {
            SetRefreshTokenCookie(loginResult.RawRefreshToken, loginResult.RefreshTokenExpiry);
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        // Refresh token'ı HttpOnly cookie'den oku
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token bulunamadı." });
        }

        var result = await _authService.RefreshToken(refreshToken, ct);

        if (!result.Success)
        {
            // Geçersiz token → cookie'yi temizle
            ClearRefreshTokenCookie();
            return Unauthorized(new { message = result.Message });
        }

        var (responseDto, newRawRefreshToken, refreshTokenExpiry) = result.Data;

        // Yeni refresh token'ı cookie'ye yaz
        SetRefreshTokenCookie(newRawRefreshToken, refreshTokenExpiry);

        return Ok(new { success = true, data = responseDto, message = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token bulunamadı." });
        }

        var result = await _authService.RevokeToken(refreshToken, ct);
        
        // Cookie'yi her durumda temizle
        ClearRefreshTokenCookie();

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    // ---------- Cookie Helpers ----------

    private void SetRefreshTokenCookie(string token, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,     // JavaScript erişemez (XSS koruması)
            Secure = false,      // Development'ta false, production'da true olmalı
            SameSite = SameSiteMode.Lax, // CSRF koruması
            Expires = expires,
            Path = "/api/auth"   // Sadece auth endpoint'lerine gönderilsin
        };

        Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth"
        });
    }
}