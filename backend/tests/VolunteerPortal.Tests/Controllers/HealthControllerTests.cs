namespace VolunteerPortal.Tests.Controllers;

/// <summary>
/// Unit tests for the HealthController.
/// </summary>
public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        // Arrange
        var controller = new VolunteerPortal.API.Controllers.HealthController();

        // Act
        var result = controller.Get();

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
        okResult.Should().NotBeNull();
        
        var healthResponse = okResult!.Value as VolunteerPortal.API.Controllers.HealthResponse;
        healthResponse.Should().NotBeNull();
        healthResponse!.Status.Should().Be("Healthy");
        healthResponse.Version.Should().NotBeNullOrEmpty();
    }
}
