using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Orsync.Filters;

/// <summary>
/// Enables Swagger to show IFormFile and List&lt;IFormFile&gt; parameters in UI.
/// </summary>
public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile)
                     || p.ParameterType == typeof(List<IFormFile>))
            .ToList();

        if (!fileParams.Any())
            return;

        var properties = fileParams
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(
                p => p.Name!,
                _ => new OpenApiSchema { Type = "string", Format = "binary" });

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
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
