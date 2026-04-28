using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;
public interface IFilesRepository : IGenericRepository<Files>
{
    Task<List<Files>> GetByOwnerAsync(string ownerType, int ownerId);
    Task<Files?> GetByHashAsync(string hash);
    Task<Files?> GetOrphanByHashAsync(string hash);
    Task<int> CountByPathAsync(string relativePath);
}