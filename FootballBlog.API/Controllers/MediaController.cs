using FootballBlog.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = "Admin")]
public class MediaController(IWebHostEnvironment env, ILogger<MediaController> logger) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<string>>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.Fail("Không có file được gửi lên."));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(ApiResponse<string>.Fail("File quá lớn. Tối đa 5MB."));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return BadRequest(ApiResponse<string>.Fail($"Định dạng không được phép. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}"));
        }

        var uploadsDir = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        logger.LogInformation("Media uploaded: {FileName} ({Size} bytes) → {Url}", file.FileName, file.Length, url);

        return Ok(ApiResponse<string>.Ok(url));
    }
}
