using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Local filesystem implementation of file storage service
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png" };

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        // Validate file
        ValidateFile(file);

        // Ensure upload directory exists
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // Generate unique filename
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // Save file
        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            
            _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
            
            // Return relative URL path
            return $"/uploads/{folder}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            throw new IOException("Failed to upload file. Please try again.", ex);
        }
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Convert relative URL path to physical path
            // filePath format: /uploads/events/guid.jpg
            var relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            // Don't throw - file deletion failure shouldn't break the flow
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public bool FileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            var relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            return File.Exists(fullPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate file size and type
    /// </summary>
    private static void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required.");
        }

        // Validate file size
        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize / 1024 / 1024}MB.");
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type '{extension}' is not allowed. Only JPG and PNG images are accepted.");
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException($"Content type '{file.ContentType}' is not allowed. Only JPEG and PNG images are accepted.");
        }
    }
}
