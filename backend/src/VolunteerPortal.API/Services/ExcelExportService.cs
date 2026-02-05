using System.Reflection;
using ClosedXML.Excel;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Service for exporting data to Excel format using ClosedXML.
/// </summary>
public class ExcelExportService : IExportService
{
    /// <summary>
    /// Exports data to Excel format (.xlsx).
    /// </summary>
    public Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName, string[]? columns = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetName);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var dataList = data.ToList();
        if (dataList.Count == 0)
        {
            // Empty export - just headers
            if (columns != null && columns.Length > 0)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = columns[i];
                }
            }
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        var type = typeof(T);
        var properties = GetPropertiesToExport(type, columns);

        // Write headers
        for (int i = 0; i < properties.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
        }

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, properties.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Write data rows
        int row = 2;
        foreach (var item in dataList)
        {
            for (int col = 0; col < properties.Count; col++)
            {
                var value = properties[col].GetValue(item);
                var cell = worksheet.Cell(row, col + 1);

                if (value == null)
                {
                    cell.Value = string.Empty;
                }
                else if (value is DateTime dateTime)
                {
                    // Excel-friendly date format
                    cell.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    cell.Value = XLCellValue.FromObject(value);
                }
            }
            row++;
        }

        // Auto-fit columns for better readability
        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return Task.FromResult(memoryStream.ToArray());
    }

    /// <summary>
    /// Gets content disposition header for timestamped file download.
    /// Format: basefilename_YYYY-MM-DD_HHmmss.xlsx
    /// </summary>
    public string GetContentDisposition(string baseFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseFileName);

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        var fileName = $"{baseFileName}_{timestamp}.xlsx";
        
        return $"attachment; filename=\"{fileName}\"";
    }

    /// <summary>
    /// Gets the list of properties to export based on column filter.
    /// </summary>
    private static List<PropertyInfo> GetPropertiesToExport(Type type, string[]? columns)
    {
        var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        if (columns == null || columns.Length == 0)
        {
            return allProperties;
        }

        var selectedProperties = new List<PropertyInfo>();
        foreach (var columnName in columns)
        {
            var property = allProperties.FirstOrDefault(p => 
                p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            
            if (property != null)
            {
                selectedProperties.Add(property);
            }
        }

        return selectedProperties;
    }
}
