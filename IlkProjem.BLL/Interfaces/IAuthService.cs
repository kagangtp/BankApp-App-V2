using IlkProjem.Core.Dtos.AuthDtos;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.BLL.Interfaces;

public interface IAuthService
{
    Task<IDataResult<LoginResponseDto>> Login(LoginDto loginDto, CancellationToken ct = default);
    Task<IResult> Register(RegisterDto registerDto, CancellationToken ct = default);
    
    /// <summary>
    /// Mevcut refresh token'ı doğrular, iptal eder ve yeni bir çift (access + refresh) döner.
    /// rawRefreshToken: Cookie'den gelen plain-text token (hash'lenmemiş)
    /// </summary>
    Task<IDataResult<(LoginResponseDto Response, string NewRefreshToken, DateTime RefreshTokenExpiry)>> RefreshToken(string rawRefreshToken, CancellationToken ct = default);
    
    /// <summary>
    /// Refresh token'ı iptal eder (logout)
    /// </summary>
    Task<IResult> RevokeToken(string rawRefreshToken, CancellationToken ct = default);
}