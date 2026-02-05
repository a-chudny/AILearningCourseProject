using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VolunteerPortal.API.Services;
using Xunit;

namespace VolunteerPortal.Tests.Services;

/// <summary>
/// Unit tests for LocalFileStorageService
/// </summary>
public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<LocalFileStorageService>> _loggerMock;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        // Create a temp directory for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"FileStorageTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);

        _environmentMock = new Mock<IWebHostEnvironment>();
        _environmentMock.Setup(e => e.WebRootPath).Returns(_tempDirectory);

        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();

        _service = new LocalFileStorageService(_environmentMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Clean up temp directory
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        GC.SuppressFinalize(this);
    }

    #region UploadAsync Tests

    [Fact]
    public async Task UploadAsync_ValidJpgFile_UploadsSuccessfully()
    {
        // Arrange
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG magic bytes
        var file = CreateMockFile("test.jpg", "image/jpeg", content);

        // Act
        var result = await _service.UploadAsync(file, "events");

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("/uploads/events/", result);
        Assert.EndsWith(".jpg", result);
        
        // Verify file was created
        var fullPath = Path.Combine(_tempDirectory, result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task UploadAsync_ValidPngFile_UploadsSuccessfully()
    {
        // Arrange
        var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic bytes
        var file = CreateMockFile("test.png", "image/png", content);

        // Act
        var result = await _service.UploadAsync(file, "events");

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(".png", result);
    }

    [Fact]
    public async Task UploadAsync_ValidJpegExtension_UploadsSuccessfully()
    {
        // Arrange
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file = CreateMockFile("test.jpeg", "image/jpeg", content);

        // Act
        var result = await _service.UploadAsync(file, "events");

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(".jpeg", result);
    }

    [Fact]
    public async Task UploadAsync_NullFile_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UploadAsync(null!, "events"));

        Assert.Contains("File is required", exception.Message);
    }

    [Fact]
    public async Task UploadAsync_EmptyFile_ThrowsArgumentException()
    {
        // Arrange
        var file = CreateMockFile("test.jpg", "image/jpeg", Array.Empty<byte>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UploadAsync(file, "events"));

        Assert.Contains("File is required", exception.Message);
    }

    [Fact]
    public async Task UploadAsync_FileTooLarge_ThrowsArgumentException()
    {
        // Arrange - 6MB file (over 5MB limit)
        var content = new byte[6 * 1024 * 1024];
        var file = CreateMockFile("large.jpg", "image/jpeg", content);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UploadAsync(file, "events"));

        Assert.Contains("exceeds the maximum allowed size", exception.Message);
    }

    [Fact]
    public async Task UploadAsync_InvalidExtension_ThrowsArgumentException()
    {
        // Arrange
        var content = new byte[] { 0x00, 0x00, 0x00 };
        var file = CreateMockFile("test.gif", "image/gif", content);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UploadAsync(file, "events"));

        Assert.Contains("File type '.gif' is not allowed", exception.Message);
    }

    [Fact]
    public async Task UploadAsync_InvalidContentType_ThrowsArgumentException()
    {
        // Arrange - Valid extension but wrong content type
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file = CreateMockFile("test.jpg", "image/gif", content);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UploadAsync(file, "events"));

        Assert.Contains("Content type", exception.Message);
    }

    [Fact]
    public async Task UploadAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file = CreateMockFile("test.jpg", "image/jpeg", content);
        var newFolder = "new_folder";

        // Act
        var result = await _service.UploadAsync(file, newFolder);

        // Assert
        var uploadDir = Path.Combine(_tempDirectory, "uploads", newFolder);
        Assert.True(Directory.Exists(uploadDir));
    }

    [Fact]
    public async Task UploadAsync_GeneratesUniqueFilename()
    {
        // Arrange
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file1 = CreateMockFile("test.jpg", "image/jpeg", content);
        var file2 = CreateMockFile("test.jpg", "image/jpeg", content);

        // Act
        var result1 = await _service.UploadAsync(file1, "events");
        var result2 = await _service.UploadAsync(file2, "events");

        // Assert - Both should succeed with different names
        Assert.NotEqual(result1, result2);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingFile_DeletesSuccessfully()
    {
        // Arrange - Upload a file first
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file = CreateMockFile("test.jpg", "image/jpeg", content);
        var uploadPath = await _service.UploadAsync(file, "events");

        var fullPath = Path.Combine(_tempDirectory, uploadPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath)); // Verify file exists

        // Act
        await _service.DeleteAsync(uploadPath);

        // Assert
        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task DeleteAsync_NonexistentFile_DoesNotThrow()
    {
        // Arrange
        var nonexistentPath = "/uploads/events/nonexistent.jpg";

        // Act & Assert - Should not throw
        await _service.DeleteAsync(nonexistentPath);
    }

    [Fact]
    public async Task DeleteAsync_NullPath_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        await _service.DeleteAsync(null!);
    }

    [Fact]
    public async Task DeleteAsync_EmptyPath_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        await _service.DeleteAsync(string.Empty);
    }

    [Fact]
    public async Task DeleteAsync_WhitespacePath_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        await _service.DeleteAsync("   ");
    }

    #endregion

    #region FileExists Tests

    [Fact]
    public async Task FileExists_ExistingFile_ReturnsTrue()
    {
        // Arrange - Upload a file first
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var file = CreateMockFile("test.jpg", "image/jpeg", content);
        var uploadPath = await _service.UploadAsync(file, "events");

        // Act
        var result = _service.FileExists(uploadPath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FileExists_NonexistentFile_ReturnsFalse()
    {
        // Arrange
        var nonexistentPath = "/uploads/events/nonexistent.jpg";

        // Act
        var result = _service.FileExists(nonexistentPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_NullPath_ReturnsFalse()
    {
        // Act
        var result = _service.FileExists(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_EmptyPath_ReturnsFalse()
    {
        // Act
        var result = _service.FileExists(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_WhitespacePath_ReturnsFalse()
    {
        // Act
        var result = _service.FileExists("   ");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((target, token) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(target, token);
            });

        return fileMock.Object;
    }

    #endregion
}
