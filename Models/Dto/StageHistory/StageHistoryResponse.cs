using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.StageHistory;

public class StageHistoryResponse
{
    public int Id { get; init; }
    public ApplicationStage FromStage { get; init; }
    public ApplicationStage ToStage { get; init; }
    public int ChangedById { get; init; }
    public string ChangedByName { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? Reason { get; init; }
}
