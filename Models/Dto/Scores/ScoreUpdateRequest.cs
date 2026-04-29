using System.ComponentModel.DataAnnotations;

namespace Aihrly.Api.Models.Dto.Scores;

public class ScoreUpdateRequest
{
    [Range(1, 5)]
    public int Score { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}
