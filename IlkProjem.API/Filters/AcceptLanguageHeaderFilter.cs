using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace IlkProjem.API.Filters;

public class AcceptLanguageHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema 
            { 
                Type = JsonSchemaType.String,
                Enum = [
                    JsonValue.Create("en-US"),
                    JsonValue.Create("tr-TR")
                ],
                Default = JsonValue.Create("en-US")
            },
            Description = "Language preference for API response messages"
        });
    }
}