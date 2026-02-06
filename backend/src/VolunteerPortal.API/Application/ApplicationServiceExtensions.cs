namespace VolunteerPortal.API.Application;

/// <summary>
/// Extension methods for registering application layer services.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers all application layer services including MediatR and AutoMapper.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR with all handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly));

        // Register AutoMapper with all profiles from this assembly
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);

        return services;
    }
}
