using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Aihrly.Api.Filters;
using Aihrly.Api.Models.Dto.Applications;
using Aihrly.Api.Models.Dto.Notes;
using Aihrly.Api.Models.Dto.Scores;
using Aihrly.Api.Models.Dto.StageHistory;
using Aihrly.Api.Models.Entities;
using Aihrly.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Controllers;

[ApiController]
[Route("api")]
public class ApplicationsController : ControllerBase
{
    private readonly AihrlyDbContext _dbContext;

    public ApplicationsController(AihrlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("jobs/{jobId:int}/applications")]
    public async Task<ActionResult<ApplicationProfileResponse>> CreateApplication(
        int jobId,
        [FromBody] ApplicationCreateRequest request,
        CancellationToken cancellationToken)
    {
        var jobExists = await _dbContext.Jobs.AnyAsync(job => job.Id == jobId, cancellationToken);
        if (!jobExists)
        {
            return NotFound();
        }

        var normalizedEmail = request.CandidateEmail.Trim().ToLowerInvariant();

        var duplicateExists = await _dbContext.Applications.AnyAsync(
            application => application.JobId == jobId && application.CandidateEmail == normalizedEmail,
            cancellationToken);

        if (duplicateExists)
        {
            return BadRequest(CreateProblemDetails(
                "Duplicate application",
                "A candidate cannot apply to the same job twice with the same email."));
        }

        var application = new Application
        {
            JobId = jobId,
            CandidateName = request.CandidateName.Trim(),
            CandidateEmail = normalizedEmail,
            CoverLetter = request.CoverLetter?.Trim(),
            CurrentStage = ApplicationStage.Applied
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ApplicationProfileResponse
        {
            Id = application.Id,
            JobId = application.JobId,
            CandidateName = application.CandidateName,
            CandidateEmail = application.CandidateEmail,
            CoverLetter = application.CoverLetter,
            CurrentStage = application.CurrentStage
        };

        return CreatedAtAction(nameof(GetApplicationProfile), new { id = application.Id }, response);
    }

    [HttpGet("jobs/{jobId:int}/applications")]
    public async Task<ActionResult<IReadOnlyList<ApplicationListItemResponse>>> GetApplicationsForJob(
        int jobId,
        [FromQuery] string? stage,
        CancellationToken cancellationToken)
    {
        var jobExists = await _dbContext.Jobs.AnyAsync(job => job.Id == jobId, cancellationToken);
        if (!jobExists)
        {
            return NotFound();
        }

        ApplicationStage? stageFilter = null;
        if (!string.IsNullOrWhiteSpace(stage))
        {
            if (!Enum.TryParse(stage, true, out ApplicationStage parsedStage))
            {
                return BadRequest(CreateProblemDetails(
                    "Invalid stage filter",
                    "Stage must be a valid pipeline stage value."));
            }

            stageFilter = parsedStage;
        }

        var query = _dbContext.Applications.AsNoTracking()
            .Where(application => application.JobId == jobId);

        if (stageFilter.HasValue)
        {
            query = query.Where(application => application.CurrentStage == stageFilter.Value);
        }

        var items = await query
            .OrderBy(application => application.Id)
            .Select(application => new ApplicationListItemResponse
            {
                Id = application.Id,
                CandidateName = application.CandidateName,
                CandidateEmail = application.CandidateEmail,
                CurrentStage = application.CurrentStage
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("applications/{id:int}")]
    public async Task<ActionResult<ApplicationProfileResponse>> GetApplicationProfile(
        int id,
        CancellationToken cancellationToken)
    {
        var response = await _dbContext.Applications.AsNoTracking()
            .Where(application => application.Id == id)
            .Select(application => new ApplicationProfileResponse
            {
                Id = application.Id,
                JobId = application.JobId,
                CandidateName = application.CandidateName,
                CandidateEmail = application.CandidateEmail,
                CoverLetter = application.CoverLetter,
                CurrentStage = application.CurrentStage,
                CultureFit = new ScoreDetailResponse
                {
                    Score = application.CultureFitScore,
                    Comment = application.CultureFitComment,
                    UpdatedById = application.CultureFitUpdatedById,
                    UpdatedByName = application.CultureFitUpdatedBy != null
                        ? application.CultureFitUpdatedBy.Name
                        : null,
                    UpdatedAt = application.CultureFitUpdatedAt
                },
                Interview = new ScoreDetailResponse
                {
                    Score = application.InterviewScore,
                    Comment = application.InterviewComment,
                    UpdatedById = application.InterviewUpdatedById,
                    UpdatedByName = application.InterviewUpdatedBy != null
                        ? application.InterviewUpdatedBy.Name
                        : null,
                    UpdatedAt = application.InterviewUpdatedAt
                },
                Assessment = new ScoreDetailResponse
                {
                    Score = application.AssessmentScore,
                    Comment = application.AssessmentComment,
                    UpdatedById = application.AssessmentUpdatedById,
                    UpdatedByName = application.AssessmentUpdatedBy != null
                        ? application.AssessmentUpdatedBy.Name
                        : null,
                    UpdatedAt = application.AssessmentUpdatedAt
                },
                Notes = application.Notes
                    .OrderByDescending(note => note.CreatedAt)
                    .Select(note => new ApplicationNoteResponse
                    {
                        Id = note.Id,
                        Type = note.Type,
                        Description = note.Description,
                        CreatedById = note.CreatedById,
                        CreatedByName = note.CreatedBy != null
                            ? note.CreatedBy.Name
                            : string.Empty,
                        CreatedAt = note.CreatedAt
                    })
                    .ToList(),
                StageHistory = application.StageHistoryEntries
                    .OrderBy(history => history.ChangedAt)
                    .Select(history => new StageHistoryResponse
                    {
                        Id = history.Id,
                        FromStage = history.FromStage,
                        ToStage = history.ToStage,
                        ChangedById = history.ChangedById,
                        ChangedByName = history.ChangedBy != null
                            ? history.ChangedBy.Name
                            : string.Empty,
                        ChangedAt = history.ChangedAt,
                        Reason = history.Reason
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPatch("applications/{id:int}/stage")]
    [RequireTeamMemberHeader]
    public async Task<IActionResult> ChangeStage(
        int id,
        [FromBody] ApplicationStageChangeRequest request,
        CancellationToken cancellationToken)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var fromStage = application.CurrentStage;
        var toStage = request.ToStage;

        if (!StageTransitionRules.IsValidTransition(fromStage, toStage))
        {
            return BadRequest(CreateProblemDetails(
                "Invalid stage transition",
                StageTransitionRules.BuildErrorMessage(fromStage, toStage)));
        }

        var teamMemberId = (int)HttpContext.Items[HttpContextItemKeys.TeamMemberId]!;
        var now = DateTimeOffset.UtcNow;

        application.CurrentStage = toStage;
        _dbContext.StageHistoryEntries.Add(new StageHistory
        {
            ApplicationId = application.Id,
            FromStage = fromStage,
            ToStage = toStage,
            ChangedById = teamMemberId,
            ChangedAt = now,
            Reason = request.Reason?.Trim()
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ProblemDetails CreateProblemDetails(string title, string detail)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.com/400"
        };
    }
}
