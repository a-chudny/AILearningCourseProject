using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VolunteerPortal.API.Swagger;

/// <summary>
/// Operation filter to handle file upload parameters in Swagger UI
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (!fileParameters.Any())
            return;

        // Remove any parameters that Swagger automatically generated for file parameters
        if (operation.Parameters != null)
        {
            var parametersToRemove = operation.Parameters
                .Where(p => fileParameters.Any(fp => fp.Name == p.Name))
                .ToList();
            
            foreach (var param in parametersToRemove)
            {
                operation.Parameters.Remove(param);
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name ?? "file",
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        ),
                        Required = fileParameters
                            .Where(p => !p.IsOptional)
                            .Select(p => p.Name ?? "file")
                            .ToHashSet()
                    }
                }
            }
        };
    }
}
