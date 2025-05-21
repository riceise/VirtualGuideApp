using Microsoft.AspNetCore.Mvc;


namespace Guide.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IWebHostEnvironment environment, ILogger<FilesController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file provided in upload request.");
            return BadRequest("No file provided.");
        }

        if (file.Length > 5 * 1024 * 1024) // 5MB limit
        {
            _logger.LogWarning("File {FileName} exceeds size limit of 5MB.", file.FileName);
            return BadRequest("File exceeds 5MB size limit.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Invalid file type for {FileName}. Allowed types: {AllowedTypes}", file.FileName,
                string.Join(", ", allowedExtensions));
            return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
        }

        try
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                _logger.LogInformation("Creating uploads directory at {UploadsFolder}", uploadsFolder);
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/Uploads/{uniqueFileName}";
            _logger.LogInformation("File {FileName} uploaded successfully. URL: {FileUrl}", file.FileName, fileUrl);

            return Ok(new UploadResult
            {
                FileName = file.FileName,
                Url = fileUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}.", file.FileName);
            return StatusCode(500, "Внутренняя ошибка сервера при загрузке файла.");
        }
    }
}

public class UploadResult
{
    public string FileName { get; set; }
    public string Url { get; set; }
}