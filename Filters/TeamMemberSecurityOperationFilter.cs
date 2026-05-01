using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Aihrly.Api.Filters;

public class TeamMemberSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<RequireTeamMemberHeaderAttribute>()
            .Any();

        if (!hasAttribute)
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "TeamMemberId"
                        }
                    },
                    Array.Empty<string>()
                }
            }
        };
    }
}
