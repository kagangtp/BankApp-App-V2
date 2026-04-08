using Microsoft.AspNetCore.Mvc;
using IlkProjem.BLL.Services;
using IlkProjem.Core.Dtos.FileDtos;
using Microsoft.AspNetCore.Authorization;
using IlkProjem.Core.Constants;
using IlkProjem.BLL.Interfaces;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFilesService _filesService;

    public FilesController(IFilesService filesService)
    {
        _filesService = filesService;
    }

    [Authorize(Policy = Policies.FileManagement)]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "Dosya seçilmedi." });

        try
        {
            var result = await _filesService.UploadAsync(file);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Resim yükleme hatası: " + ex.Message });
        }
    }

    [Authorize(Policy = Policies.FileManagement)]
    [HttpPut("{id}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] FileAssignDto assignDto)
    {
        var result = await _filesService.AssignOwnerAsync(id, assignDto);
        return result ? Ok(new { success = true, message = "Dosya başarıyla bağlandı." })
                      : NotFound(new { success = false, message = "Dosya bulunamadı." });
    }

    [HttpGet]
    public async Task<IActionResult> GetByOwner([FromQuery] string ownerType, [FromQuery] int ownerId)
    {
        var files = await _filesService.GetByOwnerAsync(ownerType, ownerId);
        return Ok(new { success = true, data = files });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        try
        {
            var fileRecord = await _filesService.GetFileRecordAsync(id);
            if (fileRecord == null) return NotFound();

            var bytes = await _filesService.DownloadAsync(id);
            return File(bytes, fileRecord.MimeType, fileRecord.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("Dosya bulunamadı.");
        }
    }

    [Authorize(Policy = Policies.FileManagement)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _filesService.DeleteAsync(id);
        if (!result) return NotFound(new { success = false, message = "Dosya bulunamadı veya silinemedi." });
        return Ok(new { success = true, message = "Dosya başarıyla silindi." });
    }
}