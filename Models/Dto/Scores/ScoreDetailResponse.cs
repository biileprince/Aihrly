namespace Aihrly.Api.Models.Dto.Scores;

public class ScoreDetailResponse
{
    public int? Score { get; init; }
    public string? Comment { get; init; }
    public int? UpdatedById { get; init; }
    public string? UpdatedByName { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
