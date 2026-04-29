using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Jobs;

public class JobSummaryResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public JobStatus Status { get; init; }
}
