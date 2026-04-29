using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Applications;

public class ApplicationListItemResponse
{
    public int Id { get; init; }
    public string CandidateName { get; init; } = string.Empty;
    public string CandidateEmail { get; init; } = string.Empty;
    public ApplicationStage CurrentStage { get; init; }
}
