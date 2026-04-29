using Aihrly.Api.Models.Dto.Notes;
using Aihrly.Api.Models.Dto.Scores;
using Aihrly.Api.Models.Dto.StageHistory;
using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Applications;

public class ApplicationProfileResponse
{
    public int Id { get; init; }
    public int JobId { get; init; }
    public string CandidateName { get; init; } = string.Empty;
    public string CandidateEmail { get; init; } = string.Empty;
    public string? CoverLetter { get; init; }
    public ApplicationStage CurrentStage { get; init; }

    public ScoreDetailResponse CultureFit { get; init; } = new();
    public ScoreDetailResponse Interview { get; init; } = new();
    public ScoreDetailResponse Assessment { get; init; } = new();

    public IReadOnlyList<ApplicationNoteResponse> Notes { get; init; } = Array.Empty<ApplicationNoteResponse>();
    public IReadOnlyList<StageHistoryResponse> StageHistory { get; init; } = Array.Empty<StageHistoryResponse>();
}
