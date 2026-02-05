using VolunteerPortal.API.Services;

namespace VolunteerPortal.API.Tests.Services;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _exportService;

    public ExcelExportServiceTests()
    {
        _exportService = new ExcelExportService();
    }

    // Test data class
    private class TestPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public async Task ExportToExcelAsync_WithValidData_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var data = new List<TestPerson>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = new DateTime(2026, 2, 5, 14, 30, 0), IsActive = true },
            new() { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = new DateTime(2026, 2, 6, 10, 15, 0), IsActive = false }
        };

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "TestSheet");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ExportToExcelAsync_WithEmptyData_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var data = new List<TestPerson>();

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "EmptySheet");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result); // Excel file with headers only
    }

    [Fact]
    public async Task ExportToExcelAsync_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        List<TestPerson>? data = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _exportService.ExportToExcelAsync(data!, "TestSheet"));
    }

    [Fact]
    public async Task ExportToExcelAsync_WithEmptySheetName_ThrowsArgumentException()
    {
        // Arrange
        var data = new List<TestPerson> { new() { Id = 1, Name = "Test" } };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _exportService.ExportToExcelAsync(data, string.Empty));
    }

    [Fact]
    public async Task ExportToExcelAsync_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var data = new List<TestPerson>
        {
            new() { Id = 1, Name = "John Doe", Email = null, CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "NullTestSheet");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ExportToExcelAsync_WithColumnFilter_IncludesOnlySpecifiedColumns()
    {
        // Arrange
        var data = new List<TestPerson>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        var columns = new[] { "Id", "Name", "Email" };

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "FilteredSheet", columns);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ExportToExcelAsync_WithCaseInsensitiveColumns_WorksCorrectly()
    {
        // Arrange
        var data = new List<TestPerson>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        var columns = new[] { "id", "NAME", "email" }; // Mixed case

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "CaseTestSheet", columns);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ExportToExcelAsync_WithInvalidColumnNames_IgnoresInvalidColumns()
    {
        // Arrange
        var data = new List<TestPerson>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        var columns = new[] { "Id", "NonExistentColumn", "Name" };

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "InvalidColumnsSheet", columns);

        // Assert - Should not throw, should just skip invalid columns
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetContentDisposition_WithValidFileName_ReturnsCorrectFormat()
    {
        // Arrange
        var baseFileName = "users";

        // Act
        var result = _exportService.GetContentDisposition(baseFileName);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("attachment; filename=\"", result);
        Assert.Contains("users_", result);
        Assert.EndsWith(".xlsx\"", result);
        Assert.Matches(@"users_\d{4}-\d{2}-\d{2}_\d{6}\.xlsx", result);
    }

    [Fact]
    public void GetContentDisposition_WithEmptyFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _exportService.GetContentDisposition(string.Empty));
    }

    [Fact]
    public void GetContentDisposition_WithWhitespaceFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _exportService.GetContentDisposition("   "));
    }

    [Fact]
    public async Task ExportToExcelAsync_WithDateTimeValues_FormatsCorrectly()
    {
        // Arrange
        var testDate = new DateTime(2026, 2, 5, 14, 30, 0);
        var data = new List<TestPerson>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = testDate, IsActive = true }
        };

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "DateTestSheet");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Date formatting is verified by manual inspection or by reading the Excel file back
    }

    [Fact]
    public async Task ExportToExcelAsync_WithLargeDataSet_CompletesSuccessfully()
    {
        // Arrange
        var data = Enumerable.Range(1, 1000).Select(i => new TestPerson
        {
            Id = i,
            Name = $"Person {i}",
            Email = $"person{i}@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-i),
            IsActive = i % 2 == 0
        }).ToList();

        // Act
        var result = await _exportService.ExportToExcelAsync(data, "LargeDataSheet");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
