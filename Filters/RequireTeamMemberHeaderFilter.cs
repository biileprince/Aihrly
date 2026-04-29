using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Filters;

public class RequireTeamMemberHeaderFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Team-Member-Id";
    private readonly AihrlyDbContext _dbContext;

    public RequireTeamMemberHeaderFilter(AihrlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            context.Result = CreateUnauthorizedProblem("Missing X-Team-Member-Id header.");
            return;
        }

        if (!int.TryParse(values.FirstOrDefault(), out var teamMemberId))
        {
            context.Result = CreateUnauthorizedProblem("Invalid X-Team-Member-Id header.");
            return;
        }

        var exists = await _dbContext.TeamMembers.AnyAsync(member => member.Id == teamMemberId);
        if (!exists)
        {
            context.Result = CreateUnauthorizedProblem("Unknown team member.");
            return;
        }

        context.HttpContext.Items[HttpContextItemKeys.TeamMemberId] = teamMemberId;
        await next();
    }

    private static ObjectResult CreateUnauthorizedProblem(string detail)
    {
        var problem = new ProblemDetails
        {
            Title = "Unauthorized",
            Detail = detail,
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://httpstatuses.com/401"
        };

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status
        };
    }
}
