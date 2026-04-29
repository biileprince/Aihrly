using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Aihrly.Api.Filters;
using Aihrly.Api.Models.Dto.Notes;
using Aihrly.Api.Models.Entities;
using Aihrly.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Controllers;

[ApiController]
[Route("api/applications/{applicationId:int}/notes")]
public class NotesController : ControllerBase
{
    private readonly AihrlyDbContext _dbContext;
    private readonly IApplicationProfileCache _profileCache;

    public NotesController(AihrlyDbContext dbContext, IApplicationProfileCache profileCache)
    {
        _dbContext = dbContext;
        _profileCache = profileCache;
    }

    [HttpPost]
    [RequireTeamMemberHeader]
    public async Task<ActionResult<ApplicationNoteResponse>> CreateNote(
        int applicationId,
        [FromBody] ApplicationNoteCreateRequest request,
        CancellationToken cancellationToken)
    {
        var applicationExists = await _dbContext.Applications
            .AnyAsync(application => application.Id == applicationId, cancellationToken);

        if (!applicationExists)
        {
            return NotFound();
        }

        var teamMemberId = (int)HttpContext.Items[HttpContextItemKeys.TeamMemberId]!;

        var note = new ApplicationNote
        {
            ApplicationId = applicationId,
            Type = request.Type,
            Description = request.Description.Trim(),
            CreatedById = teamMemberId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ApplicationNotes.Add(note);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _profileCache.InvalidateAsync(applicationId, cancellationToken);

        var authorName = await _dbContext.TeamMembers
            .Where(member => member.Id == teamMemberId)
            .Select(member => member.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var response = new ApplicationNoteResponse
        {
            Id = note.Id,
            Type = note.Type,
            Description = note.Description,
            CreatedById = note.CreatedById,
            CreatedByName = authorName,
            CreatedAt = note.CreatedAt
        };

        return CreatedAtAction(nameof(GetNotes), new { applicationId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationNoteResponse>>> GetNotes(
        int applicationId,
        CancellationToken cancellationToken)
    {
        var applicationExists = await _dbContext.Applications
            .AnyAsync(application => application.Id == applicationId, cancellationToken);

        if (!applicationExists)
        {
            return NotFound();
        }

        var notes = await _dbContext.ApplicationNotes.AsNoTracking()
            .Where(note => note.ApplicationId == applicationId)
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
            .ToListAsync(cancellationToken);

        return Ok(notes);
    }
}
