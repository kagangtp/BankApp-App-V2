using Google.Cloud.Storage.V1;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Exceptions;
using IlkProjem.Core.Models;
using IlkProjem.Core.Dtos.FileDtos;
using IlkProjem.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace IlkProjem.BLL.Services;

public class FilesService : IFilesService
{
    private readonly IFilesRepository _fileRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly string _projectId;
    private readonly string _bucketName;
    public FilesService(IFilesRepository fileRepository, ICustomerRepository customerRepository, IConfiguration configuration)
    {
        _fileRepository = fileRepository;
        _customerRepository = customerRepository;
        _projectId = configuration["GoogleCloud:ProjectId"] 
                     ?? throw new InvalidOperationException("GoogleCloud:ProjectId is not found.");
        
        // Hardcoded or from config, as we learned from output previously
        _bucketName = "bankapp-images-mm1d09"; 
    }

    public async Task<Files> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be empty.", nameof(file));

        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new BusinessException(Core.Enums.BusinessErrorCode.FileTooLarge, "Dosya çok büyük.");

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = file.OpenReadStream();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        var fileHash = Convert.ToHexString(hashBytes);

        var existingFile = await _fileRepository.GetByHashAsync(fileHash);
        string relativePath;
        string detectedMime;
        string publicUrl;

        bool isAlreadyInGcp = existingFile != null && !string.IsNullOrEmpty(existingFile.Metadata) && existingFile.Metadata.Contains("GCP");

        if (isAlreadyInGcp)
        {
            relativePath = existingFile!.RelativePath;
            detectedMime = existingFile.MimeType;
            publicUrl = $"https://storage.googleapis.com/{_bucketName}/{relativePath}";
        }
        else
        {
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var now = DateTime.UtcNow;
            relativePath = $"{now.Year}/{now.Month:D2}/{uniqueFileName}";
            detectedMime = file.ContentType;

            var storage = await StorageClient.CreateAsync();
            stream.Position = 0;

            await storage.UploadObjectAsync(
                _bucketName,
                relativePath,
                file.ContentType,
                stream,
                new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead }
            );

            publicUrl = $"https://storage.googleapis.com/{_bucketName}/{relativePath}";
        }

        var fileEntity = new Files
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            MimeType = detectedMime,
            RelativePath = relativePath, 
            FileSize = file.Length,
            FileHash = fileHash,
            Metadata = JsonSerializer.Serialize(new { OriginalName = file.FileName, Storage = "GCP", PublicUrl = publicUrl }),
            CreatedAt = DateTime.UtcNow
        };

        await _fileRepository.AddAsync(fileEntity);
        await _fileRepository.SaveChangesAsync();

        return fileEntity;
    }

    public async Task<byte[]> DownloadAsync(Guid fileId)
    {
        var fileRecord = await _fileRepository.GetByIdAsync(fileId);
        if (fileRecord == null) throw new BusinessException(Core.Enums.BusinessErrorCode.FileRecordNotFound, "Dosya kaydı bulunamadı.");

        if (!string.IsNullOrEmpty(fileRecord.Metadata) && fileRecord.Metadata.Contains("GCP"))
        {
            var storage = await StorageClient.CreateAsync();
            using var memoryStream = new MemoryStream();
            await storage.DownloadObjectAsync(_bucketName, fileRecord.RelativePath, memoryStream);
            return memoryStream.ToArray();
        }
        
        throw new BusinessException(Core.Enums.BusinessErrorCode.FileRecordNotFound, "Dosya GCP deponuzda bulunamadı (Eski Supabase kayıtları desteklenmiyor).");
    }

    public async Task<bool> DeleteAsync(Guid fileId)
    {
        var fileRecord = await _fileRepository.GetByIdAsync(fileId);
        if (fileRecord == null) return false;

        var usageCount = await _fileRepository.CountByPathAsync(fileRecord.RelativePath);

        if (usageCount <= 1)
        {
            if (!string.IsNullOrEmpty(fileRecord.Metadata) && fileRecord.Metadata.Contains("GCP"))
            {
                var storage = await StorageClient.CreateAsync();
                await storage.DeleteObjectAsync(_bucketName, fileRecord.RelativePath);
            }
        }

        _fileRepository.Delete(fileRecord);
        await _fileRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignOwnerAsync(Guid fileId, FileAssignDto assignDto)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null) return false;

        // --- DEDUPLICATION (Aynı resim 2 kez gelmesin) ---
        // 1. Array bazlı assetler için (House, Car vb.)
        var existingOwnerFiles = await _fileRepository.GetByOwnerAsync(assignDto.OwnerType, assignDto.OwnerId);
        if (existingOwnerFiles.Any(f => f.FileHash == file.FileHash && f.Id != file.Id))
        {
            _fileRepository.Delete(file);
            await _fileRepository.SaveChangesAsync();
            return true; // İstek başarılı varsayılır ama 2. bir kopyası eklenmez, varolan resim durur.
        }

        // 2. Customer tekil profil resmi kontrolü (Eski sistemi korumak adına)
        if (assignDto.OwnerType == "Customer")
        {
            var customer = await _customerRepository.GetByIdAsync(assignDto.OwnerId);
            if (customer != null)
            {
                // Müşterinin zaten bir resmi varsa ve yeni yüklenenle birebir aynıysa:
                if (customer.ProfileImage != null && customer.ProfileImage.FileHash == file.FileHash && customer.ProfileImage.Id != file.Id)
                {
                    // Yeni yüklemeyi çöpe at, varolan kalsın!
                    _fileRepository.Delete(file);
                    await _fileRepository.SaveChangesAsync();
                    return true;
                }
                
                // --- UPDATE İŞLEMİ ---
                var oldFileId = customer.ProfileImageId;
                
                file.OwnerId = assignDto.OwnerId;
                file.OwnerType = assignDto.OwnerType;
                
                customer.ProfileImageId = fileId;
                await _customerRepository.UpdateAsync(customer);
                
                // Eğer farklı bir resme geçtiyse eski resmi sistemden temizle ki usage leak olmasın
                if (oldFileId.HasValue && oldFileId.Value != fileId)
                {
                    await DeleteAsync(oldFileId.Value);
                }
                
                await _fileRepository.SaveChangesAsync();
                return true;
            }
        }

        // --- CAR & HOUSE DEFAULT ASSIGN ---
        file.OwnerId = assignDto.OwnerId;
        file.OwnerType = assignDto.OwnerType;

        _fileRepository.Update(file);
        await _fileRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<FileReadDto>> GetByOwnerAsync(string ownerType, int ownerId)
    {
        var files = await _fileRepository.GetByOwnerAsync(ownerType, ownerId);
        
        // Sadece unique olan dosyaları (aynı FileHash'e sahip kopya kayıtlar varsa filtrele) dön
        var uniqueFiles = files
            .GroupBy(f => string.IsNullOrEmpty(f.FileHash) ? f.Id.ToString() : f.FileHash)
            .Select(g => g.First())
            .ToList();

        return uniqueFiles.Select(f => new FileReadDto
        {
            Id = f.Id,
            FileName = f.FileName,
            MimeType = f.MimeType,
            RelativePath = GetPublicUrl(f)
        }).ToList();
    }

    public string GetPublicUrl(Files file)
    {
        if (file == null) return string.Empty;

        // Eger fotoğraf GCP ise
        if (!string.IsNullOrEmpty(file.Metadata) && file.Metadata.Contains("GCP"))
        {
            return $"https://storage.googleapis.com/{_bucketName}/{file.RelativePath}";
        }
        
        // Supabase resimleri artık tamamen pas geçiliyor
        return string.Empty;
    }

    public async Task<Files?> GetFileRecordAsync(Guid fileId)
    {
        return await _fileRepository.GetByIdAsync(fileId);
    }
}