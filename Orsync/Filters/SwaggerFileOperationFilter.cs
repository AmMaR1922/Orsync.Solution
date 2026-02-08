 
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Orsync.Filters
{
    using Microsoft.OpenApi.Models;

    //public class SwaggerFileOperationFilter : IOperationFilter
    //{
    //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    //    {
    //        var fileParameters = context.MethodInfo.GetParameters()
    //            .Where(p => p.ParameterType == typeof(IFormFile) ||
    //                        p.ParameterType == typeof(List<IFormFile>) ||
    //                        p.ParameterType == typeof(IFormFile[]))
    //            .ToList();

    //        if (!fileParameters.Any())
    //            return;

    //        operation.RequestBody = new OpenApiRequestBody
    //        {
    //            Content = new Dictionary<string, OpenApiMediaType>
    //            {
    //                ["multipart/form-data"] = new OpenApiMediaType
    //                {
    //                    Schema = new OpenApiSchema
    //                    {
    //                        Type = "object",
    //                        Properties = fileParameters.ToDictionary(
    //                            p => p.Name!,
    //                            p => p.ParameterType == typeof(List<IFormFile>) || p.ParameterType == typeof(IFormFile[])
    //                                ? new OpenApiSchema
    //                                {
    //                                    Type = "array",
    //                                    Items = new OpenApiSchema
    //                                    {
    //                                        Type = "string",
    //                                        Format = "binary"
    //                                    }
    //                                }
    //                                : new OpenApiSchema
    //                                {
    //                                    Type = "string",
    //                                    Format = "binary"
    //                                }
    //                        ),
    //                        Required = fileParameters.Select(p => p.Name!).ToHashSet()
    //                    }
    //                }
    //            }
    //        };
    //    }
    //}


    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Linq;

    namespace Orsync.Filters
    {
        /// <summary>
        /// Enables Swagger to show IFormFile and List<IFormFile> parameters in UI
        /// </summary>
        public class SwaggerFileOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var fileParams = context.MethodInfo.GetParameters()
                    .Where(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile)
                             || p.ParameterType == typeof(System.Collections.Generic.List<Microsoft.AspNetCore.Http.IFormFile>))
                    .ToList();

                if (!fileParams.Any())
                    return;

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = fileParams.ToDictionary(
                                p => p.Name,
                                p => new OpenApiSchema { Type = "string", Format = "binary" })
                        }
                    }
                }
                };
            }
        }
    }

}
