using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;

namespace VolunteerPortal.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that uses InMemory database for integration tests.
/// The InMemoryDatabaseRoot ensures all tests share the same database instance.
/// Uses environment variables for JWT configuration to ensure they are available
/// before the host is built.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    public CustomWebApplicationFactory()
    {
        // Set JWT configuration via environment variables BEFORE host is built.
        // Program.cs reads configuration at startup, so we must set these early.
        Environment.SetEnvironmentVariable("Jwt__Secret", "TestSecretKey_AtLeast32CharactersLong_ForIntegrationTests!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "VolunteerPortal.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "VolunteerPortal.Tests");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "InMemory");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related registrations
            var descriptorsToRemove = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                     d.ServiceType == typeof(ApplicationDbContext) ||
                     d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add In-Memory database for testing with shared root
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb", DatabaseRoot);
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Ensures the database schema is created. Call this after creating the factory.
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    /// <summary>
    /// Clears all data from the database for test isolation.
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.RemoveRange(db.Users);
        db.Events.RemoveRange(db.Events);
        db.Registrations.RemoveRange(db.Registrations);
        db.Skills.RemoveRange(db.Skills);
        db.SaveChanges();
    }
}
