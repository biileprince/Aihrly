using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Aihrly.Api.Filters;
using Aihrly.Api.Models.Dto.Scores;
using Aihrly.Api.Models.Entities;
using Aihrly.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Controllers;

[ApiController]
[Route("api/applications/{applicationId:int}/scores")]
public class ScoresController : ControllerBase
{
    private readonly AihrlyDbContext _dbContext;
    private readonly IApplicationProfileCache _profileCache;

    public ScoresController(AihrlyDbContext dbContext, IApplicationProfileCache profileCache)
    {
        _dbContext = dbContext;
        _profileCache = profileCache;
    }

    [HttpPut("culture-fit")]
    [RequireTeamMemberHeader]
    public Task<IActionResult> UpdateCultureFit(
        int applicationId,
        [FromBody] ScoreUpdateRequest request,
        CancellationToken cancellationToken)
    {
        return UpdateScore(applicationId, request, (application, memberId, now) =>
        {
            application.CultureFitScore = request.Score;
            application.CultureFitComment = request.Comment?.Trim();
            application.CultureFitUpdatedById = memberId;
            application.CultureFitUpdatedAt = now;
        }, cancellationToken);
    }

    [HttpPut("interview")]
    [RequireTeamMemberHeader]
    public Task<IActionResult> UpdateInterview(
        int applicationId,
        [FromBody] ScoreUpdateRequest request,
        CancellationToken cancellationToken)
    {
        return UpdateScore(applicationId, request, (application, memberId, now) =>
        {
            application.InterviewScore = request.Score;
            application.InterviewComment = request.Comment?.Trim();
            application.InterviewUpdatedById = memberId;
            application.InterviewUpdatedAt = now;
        }, cancellationToken);
    }

    [HttpPut("assessment")]
    [RequireTeamMemberHeader]
    public Task<IActionResult> UpdateAssessment(
        int applicationId,
        [FromBody] ScoreUpdateRequest request,
        CancellationToken cancellationToken)
    {
        return UpdateScore(applicationId, request, (application, memberId, now) =>
        {
            application.AssessmentScore = request.Score;
            application.AssessmentComment = request.Comment?.Trim();
            application.AssessmentUpdatedById = memberId;
            application.AssessmentUpdatedAt = now;
        }, cancellationToken);
    }

    private async Task<IActionResult> UpdateScore(
        int applicationId,
        ScoreUpdateRequest request,
        Action<Application, int, DateTimeOffset> apply,
        CancellationToken cancellationToken)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(entity => entity.Id == applicationId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var teamMemberId = (int)HttpContext.Items[HttpContextItemKeys.TeamMemberId]!;
        var now = DateTimeOffset.UtcNow;

        apply(application, teamMemberId, now);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _profileCache.InvalidateAsync(applicationId, cancellationToken);

        return NoContent();
    }
}
