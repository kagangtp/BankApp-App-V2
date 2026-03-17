using FileSignatures;
using IlkProjem.Core.Enums;
using IlkProjem.Core.Exceptions;
using IlkProjem.Core.Models;
using IlkProjem.Core.Dtos.FileDtos;
using IlkProjem.DAL.Interfaces;
using Microsoft.AspNetCore.Http;    
using System.Text.Json;
using Supabase; // Supabase client için

namespace IlkProjem.BLL.Services;

public class FilesService : IFilesService
{
    private readonly IFilesRepository _fileRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly Supabase.Client _supabase;
    private const string BucketName = "media"; // Supabase'deki bucket adın

    public FilesService(IFilesRepository fileRepository, ICustomerRepository customerRepository, Supabase.Client supabase)
    {
        _fileRepository = fileRepository;
        _customerRepository = customerRepository;
        _supabase = supabase;
    }

    public async Task<Files> UploadAsync(IFormFile file)
    {
        // 1. Boyut Kontrolü
        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new BusinessException(BusinessErrorCode.FileTooLarge, "Dosya çok büyük.");

        // 2. Hash Hesaplama (Mükerrer dosya kontrolü için şart)
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = file.OpenReadStream();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        var fileHash = Convert.ToHexString(hashBytes);

        // 3. Mükerrer Kontrolü (Deduplication)
        var existingFile = await _fileRepository.GetByHashAsync(fileHash);
        string relativePath;
        string detectedMime;

        if (existingFile != null)
        {
            // Dosya zaten Supabase'de var! Sadece DB'deki yolu alıyoruz.
            relativePath = existingFile.RelativePath;
            detectedMime = existingFile.MimeType;
        }
        else
        {
            // Dosya yeni! MIME tipini belirle ve Supabase'e at.
            var inspector = new FileFormatInspector();
            stream.Position = 0;
            var format = inspector.DetermineFileFormat(stream);
            detectedMime = format?.MediaType ?? "application/octet-stream";

            var now = DateTime.UtcNow;
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Supabase/S3 formatında klasör yapısı (Forward slash '/' kullanılır)
            relativePath = $"{now.Year}/{now.Month:D2}/{uniqueFileName}";

            // Stream'i byte array'e çevirip gönderiyoruz
            using var memoryStream = new MemoryStream();
            stream.Position = 0;
            await stream.CopyToAsync(memoryStream);

            // SUPABASE UPLOAD
            await _supabase.Storage.From(BucketName).Upload(memoryStream.ToArray(), relativePath);
        }

        // DB'ye yeni bir kayıt (pointer) atıyoruz
        var fileEntity = new Files
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            MimeType = detectedMime,
            RelativePath = relativePath, // Supabase içindeki path
            FileSize = file.Length,
            FileHash = fileHash,
            Metadata = JsonSerializer.Serialize(new { OriginalName = file.FileName, Storage = "Supabase" }),
            CreatedAt = DateTime.UtcNow
        };

        await _fileRepository.AddAsync(fileEntity);
        await _fileRepository.SaveChangesAsync();

        return fileEntity;
    }

    public async Task<byte[]> DownloadAsync(Guid fileId)
    {
        var fileRecord = await _fileRepository.GetByIdAsync(fileId);
        if (fileRecord == null) throw new BusinessException(BusinessErrorCode.FileRecordNotFound, "Dosya kaydı bulunamadı.");

        // SUPABASE DOWNLOAD (Byte array olarak çekiyoruz)
        return await _supabase.Storage.From(BucketName).Download(fileRecord.RelativePath, null);
    }

    public async Task<bool> DeleteAsync(Guid fileId)
    {
        var fileRecord = await _fileRepository.GetByIdAsync(fileId);
        if (fileRecord == null) return false;

        // Bu yolu kullanan başka kayıt var mı? (Deduplication kontrolü)
        var usageCount = await _fileRepository.CountByPathAsync(fileRecord.RelativePath);

        // Eğer son kullanıcıysan fiziksel dosyayı Supabase'den sil
        if (usageCount <= 1)
        {
            await _supabase.Storage.From(BucketName).Remove(new List<string> { fileRecord.RelativePath });
        }

        _fileRepository.Delete(fileRecord);
        await _fileRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignOwnerAsync(Guid fileId, FileAssignDto assignDto)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null) return false;

        file.OwnerId = assignDto.OwnerId;
        file.OwnerType = assignDto.OwnerType;

        _fileRepository.Update(file);

        if (assignDto.OwnerType == "Customer")
        {
            var customer = await _customerRepository.GetByIdAsync(assignDto.OwnerId);
            if (customer != null)
            {
                customer.ProfileImageId = fileId;
                await _customerRepository.UpdateAsync(customer);
            }
        }

        await _fileRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<FileReadDto>> GetByOwnerAsync(string ownerType, int ownerId)
    {
        var files = await _fileRepository.GetByOwnerAsync(ownerType, ownerId);
        
        // Not: Burada URL'i frontend'e verirken GetPublicUrl eklemek isteyebilirsin
        return files.Select(f => new FileReadDto
        {
            Id = f.Id,
            FileName = f.FileName,
            MimeType = f.MimeType,
            RelativePath = f.RelativePath,
            // Eğer tam URL lazımsa:
            // PublicUrl = _supabase.Storage.From(BucketName).GetPublicUrl(f.RelativePath)
        }).ToList();
    }

    public async Task<Files?> GetFileRecordAsync(Guid fileId)
    {
        return await _fileRepository.GetByIdAsync(fileId);
    }
}