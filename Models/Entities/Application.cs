namespace Aihrly.Api.Models.Entities;

public class Application
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public Job? Job { get; set; }

    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public ApplicationStage CurrentStage { get; set; }

    public int? CultureFitScore { get; set; }
    public string? CultureFitComment { get; set; }
    public int? CultureFitUpdatedById { get; set; }
    public TeamMember? CultureFitUpdatedBy { get; set; }
    public DateTimeOffset? CultureFitUpdatedAt { get; set; }

    public int? InterviewScore { get; set; }
    public string? InterviewComment { get; set; }
    public int? InterviewUpdatedById { get; set; }
    public TeamMember? InterviewUpdatedBy { get; set; }
    public DateTimeOffset? InterviewUpdatedAt { get; set; }

    public int? AssessmentScore { get; set; }
    public string? AssessmentComment { get; set; }
    public int? AssessmentUpdatedById { get; set; }
    public TeamMember? AssessmentUpdatedBy { get; set; }
    public DateTimeOffset? AssessmentUpdatedAt { get; set; }

    public ICollection<ApplicationNote> Notes { get; set; } = new List<ApplicationNote>();
    public ICollection<StageHistory> StageHistoryEntries { get; set; } = new List<StageHistory>();
}
