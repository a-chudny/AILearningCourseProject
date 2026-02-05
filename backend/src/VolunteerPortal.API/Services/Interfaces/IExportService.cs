namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service interface for exporting data to Excel format.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a collection of data to Excel format (.xlsx).
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="data">Collection of data to export.</param>
    /// <param name="sheetName">Name of the Excel worksheet.</param>
    /// <param name="columns">Array of property names to include in the export. If null or empty, all properties are included.</param>
    /// <returns>Excel file as byte array.</returns>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName, string[]? columns = null);

    /// <summary>
    /// Gets the content disposition header value for file download.
    /// </summary>
    /// <param name="baseFileName">Base name for the file (without extension).</param>
    /// <returns>Content disposition header value with timestamped filename.</returns>
    string GetContentDisposition(string baseFileName);
}
