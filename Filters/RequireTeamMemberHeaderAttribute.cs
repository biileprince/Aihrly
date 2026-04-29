using Microsoft.AspNetCore.Mvc;

namespace Aihrly.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireTeamMemberHeaderAttribute : TypeFilterAttribute
{
    public RequireTeamMemberHeaderAttribute()
        : base(typeof(RequireTeamMemberHeaderFilter))
    {
    }
}
