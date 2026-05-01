using Aihrly.Api.Data;
using Aihrly.Api.Filters;
using Aihrly.Api.Models.Dto;
using Aihrly.Api.Models.Dto.Jobs;
using Aihrly.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Controllers;

/// <summary>Manage job postings.</summary>
[ApiController]
[Route("api/jobs")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly AihrlyDbContext _dbContext;

    public JobsController(AihrlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Create a new job posting.</summary>
    /// <remarks>Requires X-Team-Member-Id header.</remarks>
    [HttpPost]
    [RequireTeamMemberHeader]
    [ProducesResponseType(typeof(JobDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobDetailResponse>> CreateJob(
        [FromBody] JobCreateRequest request,
        CancellationToken cancellationToken)
    {
        var job = new Job
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Status = request.Status ?? JobStatus.Open
        };

        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new JobDetailResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Status = job.Status
        };

        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, response);
    }

    /// <summary>List jobs with optional status filter and pagination.</summary>
    /// <param name="status">Filter by job status: <c>Open</c> or <c>Closed</c>.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JobSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<JobSummaryResponse>>> GetJobs(
        [FromQuery] JobStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid pagination",
                Detail = "Page and pageSize must be greater than zero.",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400"
            });
        }

        var query = _dbContext.Jobs.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(job => job.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(job => job.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(job => new JobSummaryResponse
            {
                Id = job.Id,
                Title = job.Title,
                Location = job.Location,
                Status = job.Status
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<JobSummaryResponse>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

        return Ok(result);
    }

    /// <summary>Get a single job by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(JobDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobDetailResponse>> GetJobById(
        int id,
        CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs.AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        var response = new JobDetailResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Status = job.Status
        };

        return Ok(response);
    }
}
