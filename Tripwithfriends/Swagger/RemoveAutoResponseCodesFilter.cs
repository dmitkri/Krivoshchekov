using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tripwithfriends.Swagger;

public class RemoveAutoResponseCodesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null || !operation.Responses.Any())
        {
            return;
        }

        var explicitResponseCodes = new HashSet<string>();
        var methodInfo = context.MethodInfo;
        var producesResponseTypeAttributes = methodInfo
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute), true)
            .Cast<Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute>();

        foreach (var attr in producesResponseTypeAttributes)
        {
            var statusCode = attr.StatusCode.ToString();
            explicitResponseCodes.Add(statusCode);
        }

        if (explicitResponseCodes.Any())
        {
            var keysToRemove = operation.Responses
                .Where(r => !explicitResponseCodes.Contains(r.Key))
                .Select(r => r.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                operation.Responses.Remove(key);
            }
        }
    }
}

