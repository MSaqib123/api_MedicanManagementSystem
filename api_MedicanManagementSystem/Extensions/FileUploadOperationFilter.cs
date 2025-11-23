//using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;

//public class FileUploadOperationFilter : IOperationFilter
//{
//    public void Apply(OpenApiOperation operation, OperationFilterContext context)
//    {
//        // Find any parameters of type IFormFile or IEnumerable<IFormFile>
//        var fileParams = context.MethodInfo
//            .GetParameters()
//            .Where(p => p.ParameterType == typeof(IFormFile)
//                     || p.ParameterType == typeof(List<IFormFile>)
//                     || p.ParameterType == typeof(IFormFileCollection));

//        if (!fileParams.Any())
//            return;

//        // Remove default parameters (Swagger can’t handle them)
//        operation.Parameters.Clear();

//        operation.RequestBody = new OpenApiRequestBody
//        {
//            Content =
//            {
//                ["multipart/form-data"] = new OpenApiMediaType
//                {
//                    Schema = new OpenApiSchema
//                    {
//                        Type = "object",
//                        Properties = fileParams.ToDictionary(
//                            p => p.Name!,
//                            p => new OpenApiSchema
//                            {
//                                Type = "string",
//                                Format = "binary"
//                            }
//                        ),
//                        Required = fileParams.Select(p => p.Name!).ToHashSet()
//                    }
//                }
//            }
//        };
//    }
//}
