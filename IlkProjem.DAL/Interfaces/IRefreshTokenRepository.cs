using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string hashedToken, CancellationToken ct = default);
}
