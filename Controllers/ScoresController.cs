using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Aihrly.Api.Filters;
using Aihrly.Api.Models.Dto.Scores;
using Aihrly.Api.Models.Entities;
using Aihrly.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Controllers;

/// <summary>Set culture-fit, interview, and assessment scores on an application.</summary>
/// <remarks>
/// All three endpoints use PUT semantics — submitting again overwrites the previous value.
/// Scores must be between 1 and 5. All endpoints require X-Team-Member-Id header.
/// </remarks>
[ApiController]
[Route("api/applications/{applicationId:int}/scores")]
[Produces("application/json")]
public class ScoresController : ControllerBase
{
    private readonly AihrlyDbContext _dbContext;
    private readonly IApplicationProfileCache _profileCache;

    public ScoresController(AihrlyDbContext dbContext, IApplicationProfileCache profileCache)
    {
        _dbContext = dbContext;
        _profileCache = profileCache;
    }

    /// <summary>Set or overwrite the culture-fit score (1–5).</summary>
    [HttpPut("culture-fit")]
    [RequireTeamMemberHeader]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>Set or overwrite the interview score (1–5).</summary>
    [HttpPut("interview")]
    [RequireTeamMemberHeader]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>Set or overwrite the assessment score (1–5).</summary>
    [HttpPut("assessment")]
    [RequireTeamMemberHeader]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
