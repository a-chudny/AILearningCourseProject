namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service for handling file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file to the specified folder
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">The folder to store the file in</param>
    /// <returns>The relative URL path to the uploaded file</returns>
    Task<string> UploadAsync(IFormFile file, string folder);

    /// <summary>
    /// Delete a file at the specified path
    /// </summary>
    /// <param name="filePath">The relative file path to delete</param>
    Task DeleteAsync(string filePath);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    /// <param name="filePath">The relative file path to check</param>
    /// <returns>True if file exists, false otherwise</returns>
    bool FileExists(string filePath);
}
