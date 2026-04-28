using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using IlkProjem.DAL.Interfaces;

namespace IlkProjem.DAL.Repositories;

public class FilesRepository : GenericRepository<Files>, IFilesRepository
{
    public FilesRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Files>> GetByOwnerAsync(string ownerType, int ownerId)
    {
        return await _context.Files
            .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Files?> GetByHashAsync(string hash)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.FileHash == hash);
    }

    public async Task<int> CountByPathAsync(string relativePath)
    {
        return await _context.Files.CountAsync(f => f.RelativePath == relativePath);
    }

    public async Task<Files?> GetOrphanByHashAsync(string hash)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.FileHash == hash && f.OwnerId == null);
    }
}