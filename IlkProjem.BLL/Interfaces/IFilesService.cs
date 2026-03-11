using Microsoft.AspNetCore.Http;
using IlkProjem.Core.Models;
using IlkProjem.Core.Dtos.FileDtos;

namespace IlkProjem.BLL.Services;

public interface IFilesService
{
    Task<Files> UploadAsync(IFormFile file);
    Task<byte[]> DownloadAsync(Guid fileId);
    Task<bool> DeleteAsync(Guid fileId);
    Task<bool> AssignOwnerAsync(Guid fileId, FileAssignDto assignDto);
    Task<List<FileReadDto>> GetByOwnerAsync(string ownerType, int ownerId);
    Task<Files?> GetFileRecordAsync(Guid fileId);
}