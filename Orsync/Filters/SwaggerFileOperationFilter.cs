using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Orsync.Filters;

/// <summary>
/// Enables Swagger to show file upload fields for multipart/form-data endpoints.
/// </summary>
public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile)
                     || p.ParameterType == typeof(List<IFormFile>)
                     || p.ParameterType == typeof(IFormFile[]))
            .ToList();

        if (!fileParams.Any())
            return;

        var properties = fileParams
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(
                p => p.Name!,
                p => p.ParameterType == typeof(List<IFormFile>) || p.ParameterType == typeof(IFormFile[])
                    ? new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    }
                    : new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    });

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties
                    }
                }
            }
        };
    }
}
